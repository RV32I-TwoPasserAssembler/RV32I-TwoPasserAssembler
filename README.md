---

# PicoRV32 (RV32I) Uçtan Uca Toolchain: Assembler, Linker ve FPGA Loader 🚀

Bu proje, bilgisayar mimarisi ve sistem programlama disiplinleri doğrultusunda, PicoRV32 işlemci mimarisi (RV32I alt kümesi) için sıfırdan geliştirilmiş **tam entegre bir Assembler, Bağlayıcı (Linker) ve Donanım Yükleyici (Loader) yazılım zinciridir**.

Geliştirilen bu sistem; insanların anlayabildiği assembly kaynak kodlarını (.s) şeffaf JSON tabanlı nesne dosyalarına (.o) dönüştürür, bu bağımsız modülleri bağlayarak yürütülebilir makine kodu (`program.mi`) üretir ve son olarak **hata kontrollü UART Yükleyici** aracılığıyla kodların FPGA (BRAM) üzerinde fiziksel olarak koşturulmasını sağlar.

---

## 🌟 Temel Özellikler

* **İki Geçişli (Two-Pass) Assembler ve Linker:** "İleri yönlü referans" (forward reference) problemini çözmek ve birden fazla bağımsız nesne dosyasını adres çakışması olmadan birleştirmek için iki aşamalı derleme ve bağlama algoritmaları kullanır.
* **"Beyaz Kutu" (White-Box) JSON Nesne Formatı:** Karmaşık ELF formatı yerine, yama (relocation) ve sembol süreçlerinin geliştiriciler tarafından doğrudan incelenebilmesi için şeffaf metin tabanlı JSON nesne (.o) formatı kullanır.
* **RISC-V psABI Standartlarında Relocation:** İşlemcinin opcode yapısına göre 'J' (JAL), 'B' (BRANCH) ve 'D' (32-bit Data) tipi ofset yama işlemlerini uluslararası standartlara tam uyumlu olarak gerçekleştirir.
* **Hata Kontrollü (Checksum) UART Loader:** Bilgisayar (Host) ile FPGA arasındaki seri iletişimde veri kaybını önlemek için Checksum tabanlı çift yönlü doğrulama ve el sıkışma (ACK/NACK) protokolü kullanır.
* **Bellek İçi Yazılımsal FSM (Sonlu Durum Makinesi):** FPGA BRAM'in başlangıç adresinde (0x0000) hazır bekleyen yükleyici, gelen makine kodlarını güvenli hedef adrese (Örn: 0x0400) donanımsal olarak yazar ve doğrulama sonrası işlemciyi doğrudan kullanıcı koduna fırlatır (jalr).

---

## 🏗️ Mimari İşleyiş

Sistem, yazılımın donanıma inme sürecini üç ana motor üzerinden kusursuz bir şelale mantığıyla yürütür:

### 1. Assembler Evresi (C#)

* **Pass 1 (Adres Belirleme):** Ayrıştırıcı (Parser), kodu tarayarak yerel etiketleri (label) ve bellek adreslerini Sembol Tablosuna (SYMTAB) kaydeder.
* **Pass 2 (Makine Kodu ve Relocation):** Komutlar Opcode sözlüğü ile 32-bit Hex kodlarına çevrilir. Harici semboller (`.extern`) için makine koduna "0" yazılır ve Bağlayıcı için özel modifikasyon talimatları oluşturulur.

### 2. Linker (Bağlayıcı) Evresi (C#)

* **Birleştirme (ESTAB Oluşturma):** `linker.ld` betiği üzerinden okunan taban adresler (Örn: TEXT=0x0400) kullanılarak bağımsız modüller bellek haritasına (Memory Map) yerleştirilir. Harici sembollerin mutlak adresleri Dış Sembol Tablosunda (ESTAB) toplanır.
* **Adres Bağlama (Patching):** ESTAB üzerinden hedefin adresi çekilerek bit maskeleme işlemleriyle ofset değerleri gömülür ve donanımdan bağımsız `program.mi` çıktısı üretilir.

### 3. Loader ve Donanım Evresi (Python & RISC-V Assembly)

* **Paketleme ve İletim (Host Uygulaması):** Bilgisayarda çalışan Python script'i, `program.mi` dosyasını okur. Hedef başlangıç adresini ve veri bütünlüğü için hesaplanan Checksum değerini ekleyerek paketleri UART üzerinden FPGA'e iletir.
* **Karşılama ve Yürütme (FPGA FSM):** PicoRV32 üzerindeki yükleyici, UART'tan gelen paketleri yakalayarak BRAM'e ardışık olarak yazar. Hata kontrolü eşleşirse Host'a ACK (0x06) onayı gönderilir ve işlemci Program Counter (PC) değerini yeni yüklenen kullanıcı programına kaydırarak sistemi ayağa kaldırır.

---

## 💻 Desteklenen Komutlar ve Direktifler

Sistem RV32I alt kümesini ve çoklu dosya bağlama direktiflerini desteklemektedir:

* **R-Type (Register):** `add`, `sub`, `and`, `or`
* **I-Type (Immediate):** `addi`, `lw`
* **S-Type (Store):** `sw`
* **B-Type (Branch):** `beq`, `bne`
* **J-Type (Jump):** `jal`, `jalr`
* **Assembler Direktifleri:** `.word` (statik veri), `.extern` (dış referans), `.global` (dışarıya açılan sembol).

---

## 🚀 Kurulum ve Çalıştırma

Projenin derleyici kısmı .NET SDK, donanıma aktarım kısmı ise Python gerektirmektedir.

1. `.s` uzantılı kaynak kodlarınızı Assembler motoruna vererek JSON tabanlı `.o` nesne dosyalarınızı üretin.
2. Üretilen nesne dosyalarını Linker motoruna vererek FPGA'e yüklenmeye hazır `program.mi` bellek dosyanızı elde edin.
3. Donanım bağlantınızı kurduktan sonra Host (Python) uygulamasını çalıştırarak `program.mi` dosyanızı UART üzerinden FPGA kartınıza (Örn: Tang Nano 9K) güvenle yükleyip çalıştırın.