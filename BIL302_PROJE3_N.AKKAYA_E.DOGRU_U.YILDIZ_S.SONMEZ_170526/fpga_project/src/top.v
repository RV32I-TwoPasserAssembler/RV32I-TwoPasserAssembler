module top (
    input clk,          
    input rst_n,        
    input uart_rx,      
    output uart_tx,     // DONANIM ÇIKIŞI
    output [5:0] led    
);

    reg [7:0] rst_cnt = 0;
    wire internal_rstn = rst_cnt[7];
    always @(posedge clk) if (!internal_rstn) rst_cnt <= rst_cnt + 1;
    wire global_rstn = internal_rstn && rst_n;

    wire mem_valid, mem_instr;
    reg mem_ready;
    wire [31:0] mem_addr, mem_wdata;
    wire [3:0] mem_wstrb;
    reg [31:0] mem_rdata;

    picorv32 #(
        .ENABLE_COUNTERS(0),
        .PROGADDR_RESET(32'h0000_0000) 
    ) cpu (
        .clk(clk), .resetn(global_rstn), 
        .mem_valid(mem_valid), .mem_instr(mem_instr),
        .mem_ready(mem_ready), .mem_addr(mem_addr),
        .mem_wdata(mem_wdata), .mem_wstrb(mem_wstrb),
        .mem_rdata(mem_rdata)
    );

    wire [7:0] rx_data;
    wire rx_valid;
    reg rx_ack;

    uart_rx receiver (
        .clk(clk), .rstn(global_rstn), .rx_pin(uart_rx),
        .data(rx_data), .valid(rx_valid), .ack(rx_ack)
    );

    // --- UART TX ENTEGRASYONU (GÜNCELLENDİ - GÜVENLİ LATCH) ---
    wire tx_valid;
    wire tx_ready;
    reg [7:0] tx_data_reg;  // YENİ: Veriyi sabitleyecek (Latch) olan register
    reg tx_valid_reg;
    
    assign tx_valid = tx_valid_reg;
    wire [7:0] tx_data = tx_data_reg; // YENİ: Veri artık işlemciden değil, bizim güvenli kasamızdan okunuyor

    uart_tx transmitter (
        .clk(clk), .rstn(global_rstn), .valid(tx_valid),
        .data(tx_data), .tx_pin(uart_tx), .ready(tx_ready)
    );

    // --- YENİ BOOTROM ---
    reg [31:0] memory [0:2047]; 
    initial begin
        memory[0] = 32'h10000513;
        memory[1] = 32'h10400593;
        memory[2] = 32'h00000833;
        memory[3] = 32'h32000A13;
        memory[4] = 32'h00400713;
        memory[5] = 32'h00000993;
        memory[6] = 32'h03370263;
        memory[7] = 32'h00052283;
        memory[8] = 32'hFE028EE3;
        memory[9] = 32'h0005C303;
        memory[10] = 32'h006A0023;
        memory[11] = 32'h00680833;
        memory[12] = 32'h001A0A13;
        memory[13] = 32'h00198993;
        memory[14] = 32'hFE1FF06F;
        memory[15] = 32'h32000A13;
        memory[16] = 32'h000A2683;
        memory[17] = 32'h32000A13;
        memory[18] = 32'h00400713;
        memory[19] = 32'h00000993;
        memory[20] = 32'h03370263;
        memory[21] = 32'h00052283;
        memory[22] = 32'hFE028EE3;
        memory[23] = 32'h0005C303;
        memory[24] = 32'h006A0023;
        memory[25] = 32'h00680833;
        memory[26] = 32'h001A0A13;
        memory[27] = 32'h00198993;
        memory[28] = 32'hFE1FF06F;
        memory[29] = 32'h32000A13;
        memory[30] = 32'h000A2603;
        memory[31] = 32'h00C007B3;
        memory[32] = 32'h00000713;
        memory[33] = 32'h02E68263;
        memory[34] = 32'h00052283;
        memory[35] = 32'hFE028EE3;
        memory[36] = 32'h0005C303;
        memory[37] = 32'h00660023;
        memory[38] = 32'h00680833;
        memory[39] = 32'h00160613;
        memory[40] = 32'h00170713;
        memory[41] = 32'hFE1FF06F;
        memory[42] = 32'h00052283;
        memory[43] = 32'hFE028EE3;
        memory[44] = 32'h0005C883;
        memory[45] = 32'h32400A13;
        memory[46] = 32'h010A0023;
        memory[47] = 32'h000A4803;
        memory[48] = 32'h01181863;
        memory[49] = 32'h00600913;
        memory[50] = 32'h014000EF;
        memory[51] = 32'h00078067;
        memory[52] = 32'h01500913;
        memory[53] = 32'h008000EF;
        memory[54] = 32'hF29FF06F;
        memory[55] = 32'h10C00A93;
        memory[56] = 32'h000AAB03;
        memory[57] = 32'hFE0B0EE3;
        memory[58] = 32'h10800A93;
        memory[59] = 32'h012AA023;
        memory[60] = 32'h00008067;
        
    end

    wire is_uart_rx_status = (mem_addr == 32'h0000_0100);
    wire is_uart_rx_data   = (mem_addr == 32'h0000_0104);
    wire is_uart_tx_data   = (mem_addr == 32'h0000_0108); // TX Adresi
    wire is_uart_tx_status = (mem_addr == 32'h0000_010C); // TX Durumu
    wire is_led            = (mem_addr == 32'h0000_0200); 

    reg [5:0] led_reg = 6'b000000; 
    assign led = ~led_reg; 

    always @(posedge clk) begin
        mem_ready <= 0;
        rx_ack <= 0;
        tx_valid_reg <= 0; // Her vuruşta sıfırla (Pulse)

        if (mem_valid && !mem_ready) begin
            if (is_uart_rx_status && !mem_wstrb) begin
                mem_rdata <= {31'b0, rx_valid};
                mem_ready <= 1;
            end
            else if (is_uart_rx_data && !mem_wstrb) begin
                mem_rdata <= {24'b0, rx_data};
                rx_ack <= 1; 
                mem_ready <= 1;
            end
            else if (is_uart_tx_status && !mem_wstrb) begin
                mem_rdata <= {31'b0, tx_ready};
                mem_ready <= 1;
            end
            else if (mem_wstrb) begin
                if (is_led) led_reg <= mem_wdata[5:0];
                
                // YENİ: Veriyi güvenle register'a kilitleyen blok
                if (is_uart_tx_data) begin
                    tx_data_reg <= mem_wdata[7:0]; 
                    tx_valid_reg <= 1;
                end
                
                if (mem_wstrb[0]) memory[mem_addr[12:2]] [7:0]   <= mem_wdata[7:0];
                if (mem_wstrb[1]) memory[mem_addr[12:2]] [15:8]  <= mem_wdata[15:8];
                if (mem_wstrb[2]) memory[mem_addr[12:2]] [23:16] <= mem_wdata[23:16];
                if (mem_wstrb[3]) memory[mem_addr[12:2]] [31:24] <= mem_wdata[31:24];
                mem_ready <= 1;
            end
            else begin
                mem_rdata <= memory[mem_addr[12:2]];
                mem_ready <= 1;
            end
        end
    end
endmodule