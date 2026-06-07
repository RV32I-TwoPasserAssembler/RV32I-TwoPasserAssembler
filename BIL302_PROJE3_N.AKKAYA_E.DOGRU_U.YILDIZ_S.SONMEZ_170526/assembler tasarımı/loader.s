# loader.s
# PicoRV32 Software Loader 
# RISC-V assembly ile yazılmış yazılımsal FSM tabanlı loader
#
# Bellek haritası:
# 0x0100 -> UART RX STATUS
# 0x0104 -> UART RX DATA
# 0x0108 -> UART TX DATAs
# 0x010C -> UART TX STATUS
#
# Protokol:
# Host önce 4 byte program boyutunu gönderir
# Sonra 4 byte başlangıç adresi gönderir.
# Sonra program byte'larını gönderir.
# En son checksum gönderir.
#
# Checksum = boyut byte'ları + adres byte'ları + program byte'ları mod 256
#
# ACK  = 0x06
# NACK = 0x15

.text
.org 0x0000

# ==========================================================
# STATE_INIT
# UART RX adresleri hazırlanır ve Checksum sıfırlanır.
# Loader reset sonrası bu noktadan çalışmaya başlar.
# ==========================================================
state_init:
    addi x10, x0, 256      # x10 = RX_STATUS  = 0x0100
    addi x11, x0, 260      # x11 = RX_DATA    = 0x0104
    add x16, x0, x0        # x16 = checksum = 0 (Başlangıçta sıfırla)

# ==========================================================
# STATE_PREPARE_SIZE 
# Program boyutunun 4 byte olarak alınması için hazırlık yapılır.
# Boyut byte'ları geçici olarak 0x00F0 adresinden itibaren RAM'e yazılır.
# ==========================================================
state_prepare_size:
    addi x20, x0, 800      # x20 = geçici RAM alanı = 0x0320
    addi x14, x0, 4        # x14 = okunacak boyut byte sayısı
    addi x19, x0, 0        # x19 = okunan boyut byte sayacı

# ==========================================================
# STATE_READ_SIZE (YENİ)
# Host tarafından gönderilen 4 byte program boyutu okunur.
# Her boyut byte'ı checksum hesabına eklenir.
# ==========================================================
state_read_size:
    beq x14, x19, state_size_ready

state_wait_size_byte:
    lw x5, x10, 0
    beq x5, x0, state_wait_size_byte

    lbu x6, x11, 0         # gelen boyut byte'ı
    sb x6, x20, 0          # geçici RAM alanına yaz

    add x16, x16, x6       # checksum += boyut byte'ı

    addi x20, x20, 1       # sonraki geçici byte alanı
    addi x19, x19, 1       # okunan boyut byte sayacı++
    jal x0, state_read_size

# ==========================================================
# STATE_SIZE_READY (YENİ)
# 4 byte boyut RAM'den word olarak okunur.
# ==========================================================
state_size_ready:
    addi x20, x0, 800
    lw x13, x20, 0         # x13 = TAM program boyutu 

# ==========================================================
# STATE_PREPARE_ADDRESS
# Başlangıç adresinin 4 byte olarak alınması için hazırlık yapılır.
# Geçici adres alanı (0x0320) boyut işlemi bittiği için tekrar kullanılır.
# ==========================================================
state_prepare_address:
    addi x20, x0, 800      # x20 = geçici adres alanı = 0x0320
    addi x14, x0, 4        # x14 = okunacak adres byte sayısı
    addi x19, x0, 0        # x19 = okunan adres byte sayacı

# ==========================================================
# STATE_READ_ADDRESS
# Host tarafından gönderilen 4 byte başlangıç adresi okunur.
# Her adres byte'ı checksum hesabına eklenir.
# ==========================================================
state_read_address:
    beq x14, x19, state_address_ready

state_wait_address_byte:
    lw x5, x10, 0
    beq x5, x0, state_wait_address_byte

    lbu x6, x11, 0         # gelen adres byte'ı
    sb x6, x20, 0          # geçici RAM alanına yaz

    add x16, x16, x6       # checksum += adres byte'ı

    addi x20, x20, 1       # sonraki geçici byte alanı
    addi x19, x19, 1       # okunan adres byte sayacı++
    jal x0, state_read_address

# ==========================================================
# STATE_ADDRESS_READY
# 4 byte adres RAM'den word olarak okunur.
# x12 = programa yazılacak aktif adres
# x15 = program başlangıç adresi, daha sonra jalr için saklanır.
# ==========================================================
state_address_ready:
    addi x20, x0, 800
    lw x12, x20, 0         # x12 = program başlangıç adresi
    add x15, x0, x12       # x15 = başlangıç adresini sakla

    addi x14, x0, 0        # x14 = alınan program byte sayacı

# ==========================================================
# STATE_READ_DATA
# Program byte byte UART'tan alınır ve x12 adresinden itibaren RAM'e yazılır.
# Her veri byte'ı checksum hesabına eklenir.
# ==========================================================
state_read_data:
    beq x13, x14, state_wait_checksum

state_wait_data_byte:
    lw x5, x10, 0
    beq x5, x0, state_wait_data_byte

    lbu x6, x11, 0         # gelen program byte'ı
    sb x6, x12, 0          # programı RAM'e yaz

    add x16, x16, x6       # checksum += program byte'ı

    addi x12, x12, 1       # yazma adresi++
    addi x14, x14, 1       # alınan byte sayısı++
    jal x0, state_read_data

# ==========================================================
# STATE_WAIT_CHECKSUM
# Host tarafından gönderilen checksum byte'ı beklenir.
# ==========================================================
state_wait_checksum:
    lw x5, x10, 0
    beq x5, x0, state_wait_checksum

    lbu x17, x11, 0        # x17 = host checksum

# ==========================================================
# STATE_VERIFY_CHECKSUM
# FPGA tarafında hesaplanan checksum 8 bite düşürülür.
# Host checksum ile karşılaştırılır.
# ==========================================================
state_verify_checksum:
    addi x20, x0, 804      # geçici alan = 0x0324 (Checksum alt 8-bit için)
    sb x16, x20, 0         # checksum alt 8 bit RAM'e yazılır
    lbu x16, x20, 0        # x16 = checksum mod 256

    bne x16, x17, state_send_nack

# ==========================================================
# STATE_SEND_ACK
# Checksum doğruysa host'a ACK gönderilir.
# Ardından kullanıcı programına geçilir.
# ==========================================================
state_send_ack:
    addi x18, x0, 6        # ACK = 0x06
    jal x1, state_send_response

# ==========================================================
# STATE_JUMP_PROGRAM
# PC, yüklenen programın başlangıç adresine yönlendirilir.
# ==========================================================
state_jump_program:
    jalr x0, x15, 0

# ==========================================================
# STATE_SEND_NACK
# Checksum hatalıysa host'a NACK gönderilir.
# Ardından loader başa döner.
# Host tarafındaki retry mekanizması yeni gönderimi tekrar yapabilir.
# ==========================================================
state_send_nack:
    addi x18, x0, 21       # NACK = 0x15
    jal x1, state_send_response
    jal x0, state_init

# ==========================================================
# STATE_SEND_RESPONSE
# UART TX hazır olana kadar beklenir.
# x18 içindeki cevap byte'ı TX_DATA adresine yazılır.
# ==========================================================
state_send_response:
    addi x21, x0, 268      # x21 = TX_STATUS = 0x010C

state_wait_tx_ready:
    lw x22, x21, 0
    beq x22, x0, state_wait_tx_ready

    addi x21, x0, 264      # x21 = TX_DATA = 0x0108
    sw x18, x21, 0         # ACK/NACK gönder

    jalr x0, x1, 0