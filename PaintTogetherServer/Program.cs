using System.Diagnostics;

namespace PaintTogetherServer;

class Program
{
    static unsafe void Main(string[] args)
    {



        16.isEven();
    }


}

public static class math
{
    public static unsafe bool isEven(this int x)
    {
        return (x & 0b_1) == 0;
    }
}