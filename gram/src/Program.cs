namespace gram
{
    public static class Program
    {
        static void Main()
        {
            using (Renderer app = new Renderer())
                app.Run();
        }
    }
}
