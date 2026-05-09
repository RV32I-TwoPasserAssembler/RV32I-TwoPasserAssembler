# ==========================================
# module.s - Harici İşlem Kütüphanesi
# ==========================================
.text

.global hesapla_modulu  # main.s'in görebilmesi için dışa açtık
.extern main_donus      # main.s'e geri dönmek için içeri çağırdık

hesapla_modulu:
    # main.s'ten gelen x1 ve x2'yi topla, sonucu x3'e yaz
    add x3, x1, x2      # x3 = 15 + 25 = 40
    
    # İşlem bitti, main.s'teki 'main_donus' etiketine J-Type ile geri zıpla
    jal x0, main_donus

# --- VERİ (DATA) BÖLÜMÜ ---
.data
# Bu modülün kendi verisi
.word 888