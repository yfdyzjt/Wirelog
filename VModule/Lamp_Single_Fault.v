module Lamp_Single_Fault (
    input wire clk,          
    input wire in, 
    output wire out  
);
    
    wire result = in;

    Lamp_Fault_Logic ll (
        .clk(clk),
        .in(result),
        .out(out)
    );

endmodule