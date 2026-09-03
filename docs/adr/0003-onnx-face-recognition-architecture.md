# 0003 - ONNX Face Recognition Architecture

## Status
Accepted

## Context
Sistem absensi `NETFace Attendance` membutuhkan implementasi pengenalan wajah (*face recognition*) nyata untuk menggantikan sistem *dummy* (Issue #3). Implementasi ini ditargetkan berjalan secara efisien di atas CPU pada lingkungan ASP.NET Core, memerlukan kombinasi arsitektur pendeteksi wajah (*detector*), ekstraktor fitur (*embedding*), pra-pemrosesan gambar, serta kebijakan logika bisnis terminal absensi yang kuat.

Melalui wawancara teknis mendalam (Grilling & Research), sejumlah keputusan penting telah diambil untuk menyeimbangkan aspek akurasi, performa, rekayasa perangkat lunak, dan kepastian legal (lisensi).

## Decisions

### 1. Model Selection & Licensing
- **Face Detector**: Menggunakan **YuNet** (Lisensi MIT/Apache-2.0 dari OpenCV Zoo). Model *file* `.onnx` akan dikunci pada versi/commit spesifik (misal: `2023mar`) dan *hash* file-nya didokumentasikan di *build process* untuk mencegah regresi jika *upstream* berubah.
- **Face Embedding**: **SFace** (Lisensi Apache-2.0) dipilih sebagai *starting point* yang secara legal lebih aman (karena satu ekosistem dengan YuNet), dibandingkan MobileFaceNet konvensional yang kerap terbentur ambiguitas data latih MS-Celeb-1M berlisensi *non-commercial*. Penggantian ke MobileFaceNet hanya diperbolehkan jika *provenance* data latih model spesifik telah divalidasi.

### 2. Preprocessing & Alignment Workflow
- **Channel Swapping**: Konversi RGB ke BGR (untuk detektor) hanya dilakukan sekali pada layer C# *Service*, dengan evaluasi performa *channel-swap* manual versus fungsi bawaan *ImageSharp*. Titik transisi warna (BGR untuk YuNet vs RGB/BGR untuk SFace) wajib dikomentari secara eksplisit pada baris kode.
- **Landmarks & Alignment**: 5 koordinat *landmarks* wajah bersifat **wajib**. Jika nilai kepercayaan (*confidence*) *landmark* rendah, proses dihentikan (dianggap deteksi gagal).
- **Affine Transform**: Penyejajaran (*alignment*) wajah ke kanvas 112x112 akan diimplementasikan secara murni dengan C# (tanpa pustaka *native* C++) menggunakan algoritma interpolasi dan matriks SVD (misal dengan `MathNet.Numerics`). Namun, *fallback* `OpenCvSharp` wajib tetap ada hingga hasil komputasi *pure* C# divalidasi identik secara numerik dengan output OpenCvSharp.
- **Normalization**: Vektor embedding yang dihasilkan akan menjalani **L2 Normalization** secara eksplisit di kode C# (setelah eksekusi *InferenceSession* selesai).

### 3. Matching & Threshold Calibration
- **Metrik**: Hanya menggunakan **Cosine Similarity** (yang setara dengan *Dot Product* setelah vektor dinormalisasi L2).
- **Threshold Configuration**: Dikelola via konfigurasi `appsettings.json` yang diinjeksi menggunakan `IOptionsMonitor<T>` untuk memungkinkan *live-reload* tanpa me-*restart* aplikasi.
- **Kalibrasi**: Tidak menggunakan asumsi statis 0.50. Ambang batas diukur spesifik saat tahap UAT menggunakan kumpulan foto representatif dan himpunan *negative test* untuk menghasilkan kurva analisis *False Accept Rate (FAR)* versus *False Reject Rate (FRR)*.

### 4. Business Rules & Logic Edge-cases
- **Multi-face**: Request akan ditolak sepenuhnya jika mendeteksi >1 wajah dengan respon jelas (misal: "Pastikan hanya satu orang di depan kamera") untuk mengamankan premis *walk-up attendance* 1-to-1.
- **No-face**: Mengembalikan `200 OK` dengan format terstruktur (misal: `{"status": "no_face_detected"}`) atau `422 Unprocessable Entity` (bukan `400 Bad Request`), karena request teknis sudah valid namun tidak memenuhi kriteria bisnis.
- **Unknown-face**: Menolak akses (misal `401 Unauthorized`) dan **wajib** mencatat log percobaan (*audit trail*). Sistem harus memberi sinyal peringatan jika pola *unknown-face* berurutan (*brute-force / spoofing*) terdeteksi.
- **Inactive Employee**: Menolak dengan tingkat prioritas keamanan tinggi ("Access Denied — Inactive Employee") dan memicu peringatan ke keamanan/HR. Prioritas lebih tinggi daripada *unknown-face*.
- **Error Handling**: Jika model korup atau terjadi *Out-of-Memory (OOM)*, terminal otomatis *fallback* ke otentikasi mode PIN.

### 5. Architecture, Performance, & Security
- **Model Lifetime**: `InferenceSession` ONNX Runtime dideklarasikan sebagai **Singleton** via *Dependency Injection* dengan implementasi *graceful shutdown* (`IDisposable`) dan pengecekan fail-fast kesehatan model saat *startup*.
- **Model Storage**: File `.onnx` disimpan pada direktori lokal yang dikontrol dan tidak diekspos melalui peladen statis publik HTTP.
- **Concurrency & Throttling**: Meskipun `Run()` *thread-safe*, kompetisi CPU-*bound* akan memicu perlambatan ekstrem. Maka, *throughput* dikelola via *Resource Governance* (misal: menggunakan `SemaphoreSlim`) senilai dengan batasan *core* wajar untuk menstabilkan SLA latensi absensi (< 1 detik).
- **Memory Management**: Seluruh alokasi *image buffer* mentah (`byte[]`, *tensor buffer*, gambar `ImageSharp`) di dalam RAM wajib dimusnahkan seketika menggunakan blok `using` atau pemanggilan eksplisit `Dispose()` untuk menghindari *GC pressure* masif. Pengarsipan log wajah tidak menggunakan skema enkripsi rumit (karena tidak diwajibkan) tetapi fokus pada kebijakan retensi *data minimization* yang wajar.

## Consequences
- Kebergantungan pada library native akan sangat minim jika *pure* C# *affine transform* berhasil diverifikasi, melancarkan skema *deployment container* minimalis.
- Tuntutan performa yang lebih ketat akan muncul di fase *load testing* untuk mengatur *SemaphoreSlim* paling ideal per rasio spesifikasi CPU server/terminal.
- Developer wajib menaruh kehati-hatian tingkat tinggi saat memetakan tensor urutan warna input untuk menghindari *silent logic bug* pada tingkat akurasi model wajah.
