# Kaynak: Patterson & Hennessy, RISC-V Edition (Leaf Procedure / Alt Program Mantığı)
.text
.global process_data

process_data:
    # Gelen değeri (x12) alıp üzerinde işlem yapar
    addi x13, x12, 10    # x13 = x12 + 10  (Örnek bir aritmetik işlem)
    add x12, x12, x13    # x12 = x12 + x13 (Bir sonraki tur için R-Type add testi)
    
    jalr x0, x1, 0       # Çağrıldığı yere (main_test.s içindeki loop'a) dön (I-Type test)

.data
.word 999                # Data segment kaydırma (shift) testini doğrulamak için dummy veri