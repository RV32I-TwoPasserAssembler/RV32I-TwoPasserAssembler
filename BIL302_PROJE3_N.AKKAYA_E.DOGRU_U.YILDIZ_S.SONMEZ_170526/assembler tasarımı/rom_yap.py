# rom_yap.py - Makine kodunu Verilog'a çevirir
try:
    with open('program.mi', 'r') as f:
        lines = [line.strip() for line in f if line.strip() and not line.startswith('@')]
    for i, line in enumerate(lines):
        print(f"memory[{i}] = 32'h{line};")
except Exception as e:
    print("Hata:", e)