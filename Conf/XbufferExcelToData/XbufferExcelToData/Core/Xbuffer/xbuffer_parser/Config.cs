namespace xbuffer
{
    class Config
    {
        public static string input = "";                            // 输入文件目录
        public static string template = "";                         // 模板文件
        public static string output_dir = "";                       // 输出目录
        public static string output_file = "";                      // 输出文件 (仅在打包成单个文件时需要)
        public static string suffix = ".cs";                        // 文件后缀

        public static bool load(string[] args)
        {
            foreach (var line in args)
            {
                var strs = line.Split('=');
                if (strs.Length < 2)
                    continue;

                var key = strs[0];
                var value = strs[1];

                if (key == "input") input = value;
                else if (key == "template") template = value;
                else if (key == "output_dir") output_dir = value;
                else if (key == "output_file") output_file = value;
                else if (key == "suffix") suffix = value;

                //System.Console.WriteLine(key + " = " + value);
            }

            if (input == "" || template == "" || (output_dir == "" && output_file == "") || suffix == "")
                return false;

            return true;
        }
    }
}
