using System;
using System.IO;

namespace xbuffer
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            if (!Config.load(args))
            {
                Console.WriteLine("请输入正确的参数！");
                return;
            }

            if (!Directory.Exists(Config.input))
            {
                Console.WriteLine("请输入正确的描述文件路径");
                return;
            }

            if (!File.Exists(Config.template))
            {
                Console.WriteLine("请输入正确的模板文件路径");
                return;
            }

            //修改支持指定描述文件目录形式的自动化解析
            var template_str = File.ReadAllText(Config.template);
            var template_name = Path.GetFileNameWithoutExtension(Config.template);
            if (Config.output_file == "")
            {
                Directory.CreateDirectory(Config.output_dir);
            }
            else
            {
                Directory.CreateDirectory(Path.GetDirectoryName(Config.output_file));
            }

            var files = Directory.GetFiles(Config.input, "*.xb");
            foreach(var file in files)
            {
                var proto = File.ReadAllText(file);
                var proto_classs = new Proto(proto).class_protos;
                var output = "";
                var showHead = true;
                foreach (var proto_class in proto_classs)
                {
                    if (Config.output_file == "")
                    {
                        output = Parser.parse(proto_class, template_str, showHead);
                        showHead = false;
                        File.WriteAllText(Config.output_dir + "/" + proto_class.Class_Name + Config.suffix, output);
                    }
                    else
                    {
                        output += Parser.parse(proto_class, template_str, showHead);
                        output += "\n\n";
                        showHead = false;
                    }
                }
                if (Config.output_file != "")
                    File.WriteAllText(Config.output_file + Config.suffix, output);

                Console.WriteLine(string.Format("生成完毕 input:{0}, template:{1}", Path.GetFileName(file), Path.GetFileName(Config.template)));
            }
        }
    }
}