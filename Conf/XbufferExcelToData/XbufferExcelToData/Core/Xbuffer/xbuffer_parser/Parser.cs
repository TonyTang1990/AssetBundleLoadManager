/*
 * File Name:               Parser.cs
 *
 * Description:             将类对象转化成代码文本
 * Author:                  lisiyu <576603306@qq.com>
 * Create Date:             2017/10/25
 */

namespace xbuffer
{
    public class Parser
    {
        /// <summary>
        /// 将类对象转化成代码文本
        /// </summary>
        /// <param name="proto_class">类结构</param>
        /// <param name="template_str">模板文本</param>
        /// <returns></returns>
        public static string parse(Proto_Class proto_class, string template_str, bool showHead)
        {
            var template = new XTemplate(template_str);

            template.setCondition("HEAD", showHead);
            template.setValue("#CLASS_TYPE#", proto_class.Class_Type);
            template.setValue("#CLASS_NAME#", proto_class.Class_Name);
            template.setValue("#CLASS_COMMENT#", proto_class.Class_Comment);

            template.setCondition("DESERIALIZE_CLASS", proto_class.Class_Type == "class");
            template.setCondition("SERIALIZE_CLASS", proto_class.Class_Type == "class");

            if (template.beginLoop("#VARIABLES#"))
            {
                foreach (var item in proto_class.Class_Variables)
                {
                    template.setCondition("SINGLE", !item.IsArray);
                    template.setCondition("ARRAY", item.IsArray);
                    template.setValue("#VAR_TYPE#", item.Var_Type);
                    template.setValue("#VAR_NAME#", item.Var_Name);
                    template.setValue("#VAR_COMMENT#", item.Var_Comment);
                    template.nextLoop();
                }
                template.endLoop();
            }

            if (template.beginLoop("#DESERIALIZE_PROCESS#"))
            {
                foreach (var item in proto_class.Class_Variables)
                {
                    template.setCondition("SINGLE", !item.IsArray);
                    template.setCondition("ARRAY", item.IsArray);
                    template.setValue("#VAR_TYPE#", item.Var_Type);
                    template.setValue("#VAR_NAME#", item.Var_Name);
					template.setValue("#VAR_COMMENT#", item.Var_Comment);
                    template.nextLoop();
                }
                template.endLoop();
            }

            if (template.beginLoop("#DESERIALIZE_RETURN#"))
            {
                foreach (var item in proto_class.Class_Variables)
                {
					template.setValue("#VAR_TYPE#", item.Var_Type);
                    template.setValue("#VAR_NAME#", item.Var_Name);
					template.setValue("#VAR_COMMENT#", item.Var_Comment);
                    template.nextLoop();
                }
                template.endLoop();
            }

            if (template.beginLoop("#SERIALIZE_PROCESS#"))
            {
                foreach (var item in proto_class.Class_Variables)
                {
                    template.setCondition("SINGLE", !item.IsArray);
                    template.setCondition("ARRAY", item.IsArray);
                    template.setValue("#VAR_TYPE#", item.Var_Type);
                    template.setValue("#VAR_NAME#", item.Var_Name);
					template.setValue("#VAR_COMMENT#", item.Var_Comment);
                    template.nextLoop();
                }
                template.endLoop();
            }

            return template.getContent();
        }
    }
}