namespace Anansi
{
	public class InputRequest
	{
		public string Prompt { get; }
		public string VariableName { get; }
		public InputDataType DataType { get; }

		public InputRequest(string prompt, string variableName, InputDataType dataType)
		{
			Prompt = prompt;
			VariableName = variableName;
			DataType = dataType;
		}
	}
}
