module Input_Multi #(
    parameter OUTPUT_COUNT = 2  
)(       
    input wire in,
    output wire [OUTPUT_COUNT-1:0] out
);
    
    assign out = {OUTPUT_COUNT{in}};

endmodule