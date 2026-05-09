# Kaynak: Patterson & Hennessy, RISC-V Edition, Chapter 2.8
# Test Amacı: Global Symbol Export, R-Type Data Path, I-Type Register Return
.text

.global math_leaf_procedure # Linker'ın görmesi için dışa açılan etiket

math_leaf_procedure:
    # 1. Aşama: Veri İşleme (R-Type Testi)
    # x10 (150) ve x11 (250) toplanıp x12'ye (400) yazılacak
    add x12, x10, x11    

    # 2. Aşama: Çağıran fonksiyona geri dönüş (I-Type JALR Testi)
    # x1 register'ında tutulan adrese (caller'daki sw komutuna) geri dön
    jalr x0, x1, 0       

.data
.word 999  # Data segmentin adreslemesini kontrol etmek için Dummy Veri