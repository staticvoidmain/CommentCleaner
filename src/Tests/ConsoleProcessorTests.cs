using CommentCleaner;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.IO;

namespace Tests
{
    [TestClass]
    public class ConsoleProcessorTests
    {
        [TestMethod]
        public void ProcessArgs_When_Arguments_Length_Zero_Writes_Help_To_Output_Stream()
        {
			var output = new Mock<TextWriter>(MockBehavior.Strict);
			var error = new Mock<TextWriter>(MockBehavior.Strict);

			output.Setup(tw => tw.WriteLine(It.IsAny<string>()));
			output.Setup(tw => tw.Write(It.IsAny<string>()));
			output.Setup(tw => tw.WriteLine());

			ConsoleProcessor processor = new ConsoleProcessor(output.Object, error.Object);

			processor.ProcessArgs(new string[0]);

			output.VerifyAll();
			error.VerifyAll();
        }

		[TestMethod]
		public void ProcessArgs_When_Invalid_Argument_Specified_Writes_To_Error_Stream()
		{
			var error = new Mock<TextWriter>(MockBehavior.Strict);

			error.Setup(tw => tw.WriteLine(It.IsAny<string>(), It.IsAny<object>()));

			ConsoleProcessor processor = new ConsoleProcessor(Mock.Of<TextWriter>(), error.Object);

			processor.ProcessArgs(new string[] {"--magic", "--pony"});

			error.Verify(tw => tw.WriteLine(It.IsAny<string>(), It.IsAny<object>()), Times.Exactly(2));
		}
    }
}