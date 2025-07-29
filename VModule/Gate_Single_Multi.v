module Gate_Single_Multi #(
    parameter OUTPUT_COUNT = 2
)(        
    input wire clk,
    input wire logic_reset,       
    input wire in, 
    output wire [OUTPUT_COUNT-1:0] out
);

    wire result;

    assign out = {OUTPUT_COUNT{result}};

    Gate_Single_Single gl (
        .clk(clk),
        .logic_reset(logic_reset),
        .in(in),
        .out(result)
    );

endmodule