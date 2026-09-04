# 0004 - Frontend Architecture and UI Boundaries

## Status
Accepted

## Context
Dengan dimulainya pengembangan *frontend* (React/Vite) untuk aplikasi NETFace Attendance, kita perlu mendefinisikan batasan arsitektur UI, strategi *state management*, serta bagaimana *frontend* menengahi kekurangan *endpoint* pada *backend* (API yang belum siap) tanpa melanggar prinsip *Business Rule* yang harus tetap berada di *backend*.

Keputusan ini diambil setelah sesi wawancara mendalam (*Grilling*) mengenai kebutuhan UX, batasan API, dan keamanan *kiosk*.

## Decisions

### 1. Unified SPA dengan Separated Layouts
Aplikasi akan dibangun sebagai **satu Single Page Application (SPA)** menggunakan React Router, namun dipisah secara logis dan visual ke dalam dua *layout* utama:
- `/admin/*`: Area terproteksi untuk manajemen data.
- `/kiosk/*`: Antarmuka terminal publik yang *always-on*.
Pemisahan ini memungkinkan penggunaan *state* dan *CSS scope* yang sama, namun mencegah percampuran *routing logic* yang berbahaya.

### 2. Kiosk Walk-up & Auto-capture
Terminal Kiosk tidak akan menggunakan interaksi manual (tanpa tombol "Ambil Foto"). Kamera akan terus menangkap *frame*. 
Untuk mencegah *spamming* ke *backend* API (`POST /api/recognition/attempt`), *frontend* diizinkan dan **diwajibkan** untuk mengimplementasikan **pra-deteksi ringan lokal** (misal: analisis pergerakan kanvas atau deteksi wajah *tiny-model*) sebelum mengirim *payload HTTP*. Ini murni untuk optimasi jaringan, bukan logika presisi wajah.

### 3. Face Enrollment Terpisah
Karena kompleksitas perekaman vektor wajah, alur pembuatan karyawan di UI Admin akan dipisah dari alur pendaftaran wajah (*Face Enrollment*). Admin dapat membuat data identitas karyawan terlebih dahulu, lalu melampirkan wajah di tahap/halaman yang berbeda. 

### 4. Temporary Mocking & Blockers
*Frontend* tidak boleh menunda pengembangan UI hanya karena ketiadaan *endpoint* dari *backend* (seperti `GET` daftar absensi secara masif). *Frontend* diizinkan menggunakan *Mock Data* (statis) di level *Repository/Service client-side* yang nantinya dapat diganti dengan mudah saat *endpoint backend* rilis.
- **Fallback PIN**: UI *Numpad* Kiosk tetap di-render jika backend merespons `FallbackToPin: true`, namun aksi *Submit* hanya akan menampilkan status simulasi/informasi ("Belum Tersedia") hingga API PIN benar-benar ada.

### 5. UI Dependencies
* **Validasi**: Menggunakan `react-hook-form` dan `zod` untuk memastikan validasi struktur data dilakukan sedini mungkin di sisi klien sebelum dilempar ke *backend*.
* **State & Auth**: Penyimpanan JWT Token akan menggunakan `sessionStorage` (untuk Admin) guna mengurangi risiko eksposur XSS berkepanjangan pada *browser tab* yang tertinggal.

## Consequences
- *Backend* memiliki kejelasan mengenai utang *endpoint* yang mendesak (Face Enrollment, Login Device, PIN Fallback, dan Collection Sesi Absensi).
- *Frontend* dapat berjalan secara asinkron dengan *backend* menggunakan implementasi *mock* pada area yang masih terblokir.
- Penerapan pra-deteksi wajah/gerakan lokal di Kiosk akan menambah sedikit kompleksitas instalasi *library* di sisi *frontend*, namun sangat menyelamatkan SLA latensi *backend*.
