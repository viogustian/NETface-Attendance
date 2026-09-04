# Technical Research: ONNX Face Recognition di .NET

Dokumen ini berisi riset teknis konkret untuk mengganti *dummy face recognition* dengan implementasi *real* menggunakan ONNX Runtime dan .NET, ditargetkan khusus untuk **CPU deployment**.

---

## 1. Face Detection

### A. YuNet (Rekomendasi Utama)
- **Nama Model**: YuNet (Lightweight Face Detector)
- **Sumber Resmi**: [OpenCV Zoo](https://github.com/opencv/opencv_zoo/tree/master/models/face_detection_yunet)
- **Lisensi**: MIT / Apache 2.0 (sangat aman untuk komersial).
- **Input Tensor Shape**: `[1, 3, H, W]` (contoh: `[1, 3, 320, 320]`). Mendukung ukuran dinamis.
- **Output Tensor**: Matriks ukuran `[1, N, 15]`.
  - N = jumlah deteksi (anchors).
  - 15 kolom = `[x, y, w, h, x_re, y_re, x_le, y_le, x_nt, y_nt, x_rcm, y_rcm, x_lcm, y_lcm, confidence]`. Output ini sangat berharga karena menyertakan **5 koordinat *landmarks*** untuk *face alignment*.
- **Preprocessing**: BGR channel format, input berupa float32 tanpa pembagian / mean subtraction yang rumit (tergantung versi model, umumnya menerima raw `0-255` float).
- **Postprocessing**: 
  - Filter `confidence > 0.6`.
  - Non-Maximum Suppression (NMS) dengan `IoU threshold = 0.3`.
- **Kompleksitas/Performance**: ~75K parameters, ~10ms eksekusi pada CPU standar. Sangat ringan.
- **Kelebihan**: Sangat presisi, ringan, menyediakan landmarks (wajib untuk embedding).
- **Kekurangan**: Membutuhkan parsing tensor output 15 dimensi secara manual di C#.

### B. UltraFace (version-RFB-320)
- **Nama Model**: Ultra-Light-Fast-Generic-Face-Detector-1MB
- **Sumber Resmi**: [Linzaer/Ultra-Light-Fast-Generic-Face-Detector-1MB](https://github.com/Linzaer/Ultra-Light-Fast-Generic-Face-Detector-1MB)
- **Lisensi**: MIT
- **Input Tensor Shape**: `[1, 3, 240, 320]`
- **Output Tensor**: Dua tensor: `scores` `[1, N, 2]` dan `boxes` `[1, N, 4]`.
- **Preprocessing**: RGB, ukuran `320x240`, normalisasi piksel `(p - 127.0) / 128.0`.
- **Postprocessing**: Hard-NMS.
- **Kompleksitas/Performance**: Ukuran file ~1MB, sangat cepat (< 5ms di CPU).
- **Kelebihan**: Super cepat.
- **Kekurangan**: **Tidak mengembalikan *landmarks***. Absennya landmarks membuat *face alignment* tidak bisa dilakukan, yang akan menurunkan akurasi model *face recognition* (embedding) secara drastis pada wajah yang sedikit menoleh atau miring.

---

## 2. Face Embedding

### A. MobileFaceNet (Rekomendasi Utama)
- **Nama Model**: MobileFaceNet
- **Architecture**: MobileNetV2 dengan ArcFace / CosFace loss.
- **Embedding Dimension**: 128 (untuk model orisinil) atau 512 (beberapa varian ekspor).
- **Input Shape**: `[1, 3, 112, 112]`
- **Preprocessing**: RGB format, resize ke `112x112`, normalisasi `(piksel - 127.5) / 127.5`. Harus menggunakan *affine transform* (alignment) berdasarkan titik mata/hidung dari detektor.
- **Normalization**: Mengembalikan array mentah, harus dinormalisasi L2 (`vec / ||vec||`) di post-processing.
- **Output**: Tensor `[1, 128]` float32.
- **Lisensi**: MIT (Banyak varian open-source yang melatih ulang dari awal).
- **CPU Suitability**: Sangat cocok. Ukuran file ~4MB. Waktu inferensi < 15ms.

### B. ArcFace (ResNet-50)
- **Nama Model**: LResNet50E-IR
- **Architecture**: ResNet-50 (menggunakan Additive Angular Margin Loss).
- **Embedding Dimension**: 512.
- **Input Shape**: `[1, 3, 112, 112]`
- **Preprocessing**: RGB, alignment mutlak diwajibkan (standar template ArcFace 112x112), normalisasi `(piksel - 127.5) / 127.5`.
- **Output**: Tensor `[1, 512]` float32.
- **Lisensi**: Repository asli [deepinsight/insightface](https://github.com/deepinsight/insightface) mencantumkan lisensi **Non-Commercial** untuk *pretrained models*. Penggunaan *enterprise* sangat berisiko.
- **CPU Suitability**: Buruk. Ukuran model ~170MB. Eksekusi bisa > 100ms per wajah di CPU.

---

## 3. Face Matching

### Euclidean Distance vs Cosine Similarity
- **Fakta**: Arsitektur seperti ArcFace dan MobileFaceNet melatih jaringan dengan memproyeksikan fitur wajah ke ruang bundar (hypersphere). Kecocokan diukur berdasarkan **sudut / jarak angular**, bukan jarak absolut antar titik dalam ruang kartesius.
- **Keputusan**: **Cosine Similarity** adalah metrik yang tepat dan absolut.
- **Normalization**: Vektor embedding **wajib** dilakukan normalisasi L2 (menjadikan magnitudo vektor = 1.0) sebelum dihitung. Jika kedua vektor (A dan B) sudah dinormalisasi L2, Cosine Similarity direduksi menjadi operasi **Dot Product** (`A • B`).
- **Threshold**: Berdasarkan literatur asli MobileFaceNet dan ArcFace di LFW (Labeled Faces in the Wild):
  - Skala Cosine: `[-1.0, 1.0]`. Di mana 1.0 adalah identik sempurna.
  - Threshold batas wajar: **`0.45` hingga `0.55`**.
  - Untuk absensi (Low False Acceptance Rate), threshold **`0.50`** adalah nilai default yang direkomendasikan. Jarak > 0.50 = Wajah sama.

---

## 4. .NET Integration

- **Microsoft.ML.OnnxRuntime**: Library ini sepenuhnya kompatibel, dioptimasi dengan baik, dan direkomendasikan.
- **CPU Execution Provider**: Otomatis aktif. Pastikan menggunakan tipe data `DenseTensor<float>` untuk input.
- **Inference Session Lifetime**:
  - `InferenceSession` memakan memori besar dan lambat diinisialisasi.
  - Wajib dikelola sebagai **Singleton** atau melalui `ObjectPool<InferenceSession>`.
  - Class `InferenceSession` terjamin **Thread-Safe** untuk pemanggilan metode `Run()`.
- **Image Preprocessing**:
  - `OpenCvSharp` sangat efisien (memanfaatkan C++), namun membutuhkan runtime native (`libgdiplus`, dll) yang membuat ukuran container membengkak dan deployment lintas-OS rawan masalah.
  - `SixLabors.ImageSharp` 100% managed C#. Lambat untuk filter ekstrem, namun untuk sekadar *resize*, *crop*, dan ekstraksi *dense array*, performanya sangat mumpuni.

---

## 5. Final Recommendation

Berikut adalah *stack* rekomendasi awal yang konkret dan siap diimplementasikan untuk Issue #3:

1. **Face Detection**  
   $\rightarrow$ **YuNet** (ONNX).  
   *Alasan: Cepat di CPU, ukuran 1MB, dan mengembalikan Landmarks untuk alignment. Lisensi Apache/MIT.*

2. **Face Embedding**  
   $\rightarrow$ **MobileFaceNet** (ONNX).  
   *Alasan: Eksekusi secepat kilat (~10ms) di CPU, ringan (~4MB), dan akurasi setara model kelas berat pada kondisi wajar. Tersedia di berbagai repositori MIT.*

3. **Inference**  
   $\rightarrow$ **Microsoft.ML.OnnxRuntime**  
   *Alasan: Native dukungan dari Microsoft, thread-safe, dan baku.*

4. **Image Processing**  
   $\rightarrow$ **SixLabors.ImageSharp**  
   *Alasan: 100% C# code, bebas dependensi OS/Native, mudah di-deploy di Docker/Linux.*

5. **Matching**  
   $\rightarrow$ **Cosine Similarity (Dot Product pada L2-Normalized Vector)** dengan Threshold **`0.50`**.  
   *Alasan: Metrik native dari fungsi loss ArcFace/MobileFaceNet.*

---

## 6. Open Questions / Next Steps (Perlu Diputuskan)
- **Affine Transform Implementation**: Melakukan *face alignment* membutuhkan matriks transformasi affine dari 5 koordinat YuNet ke template 112x112 MobileFaceNet. Apakah akan diimplementasikan manual secara *math-only* (menggunakan matriks SVD) atau murni center crop pada iterasi pertama?
- **Pencarian Model (.onnx)**: Tim harus mengunduh file `yunet.onnx` dan `mobilefacenet.onnx` dari repo sumber untuk dimasukkan ke direktori `assets` atau `infrastructure`.
