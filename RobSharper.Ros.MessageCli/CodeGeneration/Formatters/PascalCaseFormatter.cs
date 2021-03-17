namespace RobSharper.Ros.MessageCli.CodeGeneration.Formatters
{
    public class PascalCaseFormatter : INameFormatter
    {
        private readonly INameFormatter _camelCase = new CamelCaseFormatter();
        
        public string Format(string value)
        {
            if (string.IsNullOrEmpty(value))
                return value;

            if (value.Length == 1)
                return value.ToUpper();
            
            var camelCase = _camelCase.Format(value);
            var pascalCase = camelCase[0].ToString().ToUpper() + camelCase.Substring(1);
            
            return pascalCase;
        }
    }
}