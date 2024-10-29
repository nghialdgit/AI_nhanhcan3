namespace AI_nhanhcan3
{
    public class StepRecord
    {
        public string TT { get; set; } // Trạng thái duyệt
        public string TTK { get; set; } // Trạng thái kề
        public int K { get; set; } // k(u, v)
        public int H { get; set; } // h(v)
        public int G { get; set; } // g(v)
        public int F { get; set; } // f(v)
        public string DSL1 { get; set; } // Danh sách L1 (các đỉnh kề)
        public string DSL { get; set; } // Danh sách L (danh sách duyệt)

        public StepRecord(string tt, string ttk, int k, int h, int g, int f, string dsL1, string dsL)
        {
            TT = tt;
            TTK = ttk;
            K = k;
            H = h;
            G = g;
            F = f;
            DSL1 = dsL1;
            DSL = dsL;
        }

        public StepRecord()
        {

        }

        public override string ToString()
        {
            return $"{TT}\t{TTK}\t{K}\t{H}\t{G}\t{F}\t{DSL1}\t{DSL}";
        }
    }
}
