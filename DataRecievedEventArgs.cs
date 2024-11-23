namespace shiTTY
{
    public readonly struct DataRecievedEventArgs(char[] data, bool isError)
    {
        public char[] Data { get; } = data;
        public bool IsError { get; } = isError;
    }
}