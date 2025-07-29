module Gate_Fault_Logic (        
    input wire clk,
    input wire logic_reset,       
    input wire in, 
    output wire out
);

    reg has_out;

    initial begin
        has_out = 0;
    end

    assign out = has_out ? 0 : in;

    always @(posedge clk) begin
        if (logic_reset) begin
            has_out <= 0;
        end
        else if (out) begin
            has_out <= 1;
        end
    end

endmodule