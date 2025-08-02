module Gate_Multi_Single_Fault #(
    parameter INPUT_COUNT = 2,
    parameter RAND_SEED = 12'hAAA
)(
    input wire clk,
    input wire reset,
    input wire logic_reset,       
    input wire [INPUT_COUNT-1:0] in, 
    input wire fault_in, 
    output wire out
);

    reg [$clog2(INPUT_COUNT+1)-1:0] ones_count;
    
    integer i;
    always @(*) begin
        ones_count = 0;
        for (i = 0; i < INPUT_COUNT; i = i + 1) begin
            if (in[i]) ones_count = ones_count + 1;
        end
    end
    
    reg [11:0] lfsr_reg = RAND_SEED;
    wire lfsr_feedback = lfsr_reg[11] ^ lfsr_reg[5] ^ lfsr_reg[3] ^ lfsr_reg[0];
    
    always @(posedge clk) begin
        if (reset)
            lfsr_reg <= RAND_SEED;
        else
            lfsr_reg <= {lfsr_reg[10:0], lfsr_feedback};
    end
    
    wire random_value = (lfsr_reg < (ones_count * (12'hFFF / INPUT_COUNT)));

    wire result = fault_in & random_value;

    Gate_Fault_Logic gl (
        .clk(clk),
        .logic_reset(logic_reset),
        .in(result),
        .out(out)
    );

endmodule