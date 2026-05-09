# Kaynak: Patterson & Hennessy, RISC-V Edition, Chapter 2.8
# Test Amacı: External Symbol Resolution, J-Type Linker Relocation, S-Type Memory Store
.text
.org 0x0000

.global start
.extern math_leaf_procedure

start:
    # 1. Aşama: Parametreleri hazırlama (I-Type Testi)
    addi x10, x0, 150    # Argüman 1 (a = 150)
    addi x11, x0, 250    # Argüman 2 (b = 250)
    addi x4, x0, 2048    # Sonucun yazılacağı RAM adresi (0x0800)

    # 2. Aşama: Harici fonksiyona zıplama (J-Type Linker Yama Testi)
    jal x1, math_leaf_procedure  # Fonksiyonu çağır, dönüş adresini x1'e kaydet

    # 3. Aşama: Sonucu belleğe kaydetme (S-Type Testi)
    sw x12, x4, 0        # math_leaf_procedure'den dönen x12'deki sonucu RAM'e yaz

sonsuz_dongu:
    jal x0, sonsuz_dongu # Test bitince işlemciyi güvenli duruma al

.data
.word 0