# PicoRV (RV32I) Two-Pass Assembler 🚀

Bu proje, bilgisayar mimarisi ve sistem programlama prensipleri doğrultusunda, PicoRV işlemci mimarisi (RV32I alt kümesi) için C# kullanılarak sıfırdan geliştirilmiş *iki geçişli (two-pass) bir Assembler yazılımıdır.*

Geliştirilen bu sistem, insanların anlayabildiği assembly kaynak kodlarını (.s uzantılı) okuyup ayrıştırarak, donanım seviyesinde doğrudan yürütülebilir *32-bit Hexadecimal makine kodlarına (.hex uzantılı)* dönüştürür.

---

## 🌟 Temel Özellikler

* *İki Geçişli (Two-Pass) Mimari:* "İleri yönlü referans" (forward reference) problemini çözmek için kaynak kodu iki kez tarar.
* *O(1) Zaman Karmaşıklığı:* Sembol Tablosu (SYMTAB) ve Opcode Tablosu (OPTAB) aramaları, Hash Table (Dictionary) veri yapıları kullanılarak O(1) sabit zamanlı erişimle optimize edilmiştir.
* *RV32I Komut Seti Desteği:* R, I, S ve B komut formatlarını destekler. Bit kaydırma (bitwise shift) algoritmaları ve 2'ye tümleyen (two's complement) mantığıyla eksiksiz hesaplama yapar.
* *PC-Relative Adresleme:* Dallanma (branch) komutlarında hedef adres ile Program Sayacı (PC) arasındaki yer değiştirme (displacement) miktarı dinamik olarak hesaplanır.
* *Çapraz Doğrulama (Cross-Validation):* Üretilen makine kodları, endüstri standardı olan *Berkeley Venus RISC-V simülatörü* çıktılarıyla %100 uyuşmaktadır.

---

## 🏗️ Mimari İşleyiş

Sistem iki ana evrede çalışarak derleme işlemini gerçekleştirir:

1. *Pass 1 (Birinci Geçiş):* Ayrıştırıcı (Parser), kod içerisinde karşılaştığı etiketleri (label) ve karşılık gelen Program Counter (PC) bellek adreslerini bularak Sembol Tablosuna (SYMTAB) kaydeder. Bu aşamada makine kodu üretilmez.
2. *Pass 2 (İkinci Geçiş):* Kaynak kod tekrar taranır. Ayrıştırılan anımsatıcılar (mnemonic), sözlük (OPTAB) yardımıyla sayısal karşılıklarına çevrilir. Etiket adresleri SYMTAB üzerinden çekilir. Son olarak, bit kaydırma (<<) ve mantıksal VEYA (|) operatörleriyle parçalar üst üste bindirilip 32-bitlik nesne kodu (object code) oluşturulur.

---

## 💻 Desteklenen Komutlar

Sistem şu an temel RV32I alt kümesini desteklemektedir:
* *R-Type (Register):* add, sub, and, or
* *I-Type (Immediate):* addi, lw
* *S-Type (Store):* sw
* *B-Type (Branch):* beq, bne

---

## 🚀 Kurulum ve Çalıştırma

Proje *C#* dili kullanılarak dış bir kütüphaneye bağlı kalınmadan geliştirilmiştir. Çalıştırmak için bilgisayarınızda .NET SDK'nın kurulu olması yeterlidir.
