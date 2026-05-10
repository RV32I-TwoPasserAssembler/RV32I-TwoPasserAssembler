Raporunuzdaki yeni geliştirmeleri (Bağlayıcı/Linker mimarisi, JSON nesne dosyaları, FPGA entegrasyonu ve RISC-V psABI standartları) dikkate alarak projenizin README dosyasını kapsamlı bir araç zincirini (toolchain) yansıtacak şekilde güncelledim:

***

# PicoRV32 (RV32I) Assembler & Linker Toolchain 🚀

Bu proje, bilgisayar mimarisi ve sistem programlama disiplinleri doğrultusunda, PicoRV32 işlemci mimarisi (RV32I alt kümesi) için C# kullanılarak sıfırdan ve nesne yönelimli (OOP) geliştirilmiş **iki geçişli (two-pass) bir Assembler ve Bağlayıcı (Linker) yazılım zinciridir**. 

Geliştirilen bu sistem, insanların anlayabildiği assembly kaynak kodlarını (.s) okuyup ayrıştırarak şeffaf JSON tabanlı nesne dosyalarına (.o) dönüştürür ve ardından bu bağımsız modülleri bağlayarak FPGA BRAM belleklerine doğrudan yüklenebilecek evrensel yürütülebilir formatta (`program.mi`) makine kodu üretir.

---

## 🌟 Temel Özellikler

* **İki Geçişli (Two-Pass) Assembler ve Linker:** "İleri yönlü referans" (forward reference) problemini çözmek ve birden fazla bağımsız nesne dosyasını adres çakışması olmadan birleştirmek için iki aşamalı derleme ve bağlama algoritmaları kullanır.
* **"Beyaz Kutu" (White-Box) JSON Nesne Formatı:** Karmaşık ve okunması zor endüstriyel ELF formatı yerine, yama (relocation) ve sembol süreçlerinin öğrenciler/geliştiriciler tarafından doğrudan incelenebilmesi için şeffaf metin tabanlı JSON nesne (.o) formatı kullanır.
* **RISC-V psABI Standartlarında Relocation:** İşlemcinin opcode yapısına göre 'J' (JAL), 'B' (BRANCH) ve 'D' (32-bit Data) tipi anlık değer (immediate) yama işlemlerini, dünya çapındaki psABI matematiksel standartlarına tam uyumlu olarak gerçekleştirir.
* **O(1) Zaman Karmaşıklığı ile Sembol Çözümleme:** Assembler aşamasındaki SYMTAB/OPTAB aramaları Hash Table ile; Linker aşamasındaki modifikasyon kayıtları ise harici sembollere atanan özel indeksler (`TargetRefId`) üzerinden yapılarak metin arama maliyeti O(1) düzeyine indirilmiştir.
* **Donanımdan Bağımsız FPGA Çıktısı ($readmemh):** Xilinx veya Intel gibi donanım üreticilerine bağımlı kalmamak (vendor lock-in) için, kod ve veri bölümlerini IEEE Verilog standardı olan `@` etiketli evrensel hex formatında (`program.mi`) üreterek her türlü sentezleyicide doğrudan BRAM başlatılmasını sağlar.

---

## 🏗️ Mimari İşleyiş

Sistem, kaynak kodu donanım seviyesine indirmek için **Assembler** ve **Linker** olmak üzere iki ana motor üzerinden çalışır:

### 1. Assembler Evresi
* **Pass 1 (Adres Belirleme):** Ayrıştırıcı (Parser), kodu tarayarak yerel etiketleri (label) ve Program Counter (PC) / Data Counter adreslerini Sembol Tablosuna (SYMTAB) kaydeder.
* **Pass 2 (Makine Kodu ve Relocation Üretimi):** Komutlar Opcode sözlüğü (OPTAB) ile 32-bit Hex kodlarına çevrilir. Dosyada bulunmayan harici semboller (`.extern`) için makine koduna "0" yazılır ve Bağlayıcının (Linker) daha sonra düzeltmesi için özel modifikasyon talimatları (Relocation Records) oluşturulur.

### 2. Linker (Bağlayıcı) Evresi
* **Aşama 1 (Birleştirme ve ESTAB Oluşturma):** `linker.ld` (Linker Script) dosyasından okunan taban adresler (Örn: TEXT=0x0000, DATA=0x1000) kullanılarak bağımsız nesne modülleri bellek haritasına (Memory Map) yerleştirilir. Harici sembollerin mutlak adresleri Dış Sembol Tablosunda (ESTAB) toplanır.
* **Aşama 2 (Adres Bağlama ve Yürütme):** İlk aşamada "0" bırakılan makine kodlarına, ESTAB üzerinden hedefin adresi çekilerek bit kaydırma ve maskeleme işlemleriyle ofset değerleri gömülür (Binding/Patching). Sistem, "start" veya "main" etiketini bularak yürütme adresini (EXECADDR) belirler ve FPGA için `program.mi` çıktısını üretir.

---

## 💻 Desteklenen Komutlar ve Direktifler

Sistem RV32I alt kümesini ve çoklu dosya bağlama direktiflerini desteklemektedir:
* **R-Type (Register):** `add`, `sub`, `and`, `or`
* **I-Type (Immediate):** `addi`, `lw`
* **S-Type (Store):** `sw`
* **B-Type (Branch):** `beq`, `bne`
* **J-Type (Jump):** `jal`
* **Assembler Direktifleri:** `.word` (statik veri), `.extern` (dış referans), `.global` (dışarıya açılan sembol).

---

## 🚀 Kurulum ve Çalıştırma

Proje **C#** dili kullanılarak dış bir kütüphaneye bağlı kalınmadan geliştirilmiştir. Çalıştırmak için bilgisayarınızda .NET SDK'nın kurulu olması yeterlidir.

1. `.s` uzantılı kaynak kodlarınızı Assembler motoruna vererek JSON tabanlı `.o` nesne dosyalarınızı üretin.
2. Üretilen nesne dosyalarını Linker motoruna vererek FPGA'e yüklenmeye hazır `program.mi` (HEX) bellek haritası dosyanızı elde edin.
