using System.Text;

namespace RobSharper.Ros.MessageCli.CodeGeneration.Formatters
{
    public class CamelCaseFormatter : INameFormatter
    {
        public string Format(string value)
        {
            value = value?.Trim();
            
            if (string.IsNullOrEmpty(value))
                return value;

            var camelCase = new StringBuilder();
            
            for (var i = 0; i < value.Length; i++)
            {
                if (i == 0)
                {
                    camelCase.Append(char.ToLower(value[i]));
                }
                else
                {
                    var nextCharacterToUpperCase = false;

                    while (i < value.Length && value[i] == '_')
                    {
                        i++;
                        nextCharacterToUpperCase = true;
                    }

                    if (nextCharacterToUpperCase)
                    {
                        if (i == value.Length)
                            break;

                        camelCase.Append(char.ToUpper(value[i]));
                    }
                    else
                    {
                        camelCase.Append(value[i]);
                    }
                }
            }

            return camelCase.ToString();
        }
    }
}