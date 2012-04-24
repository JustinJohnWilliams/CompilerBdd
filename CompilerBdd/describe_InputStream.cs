using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NSpec;
using System.IO;
using System.Reflection;

namespace CompilerBdd
{
    /// <summary>
    /// <remarks>Assignment 2</remarks>
    /// Represents a class for emulating an imput stream
    /// </summary>
    public class InputStream
    {
        public char current_character { get; set; }
        public Position current_position { get; set; }
        public StreamReader current_stream { get; private set; }

        private List<string> fileLines { get; set; }

        public void open_input(string fileName)
        {
            var path = GetFullFilePath(fileName);

            if (File.Exists(path))
            {
                current_position.FileName = fileName;
                current_stream = new StreamReader(path);
            }
            else throw new FileNotFoundException("File {0} does not exist.".With(path));
        }

        public char read_current_char()
        {
            fileLines = current_stream.ReadToEnd().Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries).ToList();

            return fileLines[current_position.Line - 1].ElementAt(current_position.Column);
        }

        public void close()
        {
            if (this.current_stream != null) current_stream.Close();
        }

        private string GetFullFilePath(string fileName)
        {
            return "{0}/Data/{1}".With(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), fileName);
        }
    }

    public class describe_InputStream : nspec
    {
        dynamic inputStream;

        void before_each()
        {
            inputStream = new InputStream();
            inputStream.current_position = new Position();
        }

        void after_each()
        {
            inputStream.close();
        }

        void opening_file()
        {
            context["opening files"] = () =>
            {
                it["should set filename to testfile.txt and open file stream if file exists"] = () =>
                {
                    inputStream.open_input("testfile.txt");

                    (inputStream.current_position.FileName as string).should_be("testfile.txt");
                    (inputStream.current_stream as StreamReader).should_not_be_null();
                };

                it["should throw exception if file does not exist"] =
                    expect<FileNotFoundException>(() => inputStream.open_input(Path.GetRandomFileName()));
            };
        }

        void closing_file()
        {
            context["closing a file stream"] = () =>
            {
                it["should close a file stream"] = () =>
                {
                    inputStream.open_input("testfile.txt");

                    (inputStream.current_stream as StreamReader).should_not_be_null();

                    inputStream.close();

                    inputStream.open_input("testfile.txt");

                    (inputStream.current_stream as StreamReader).should_not_be_null();
                };
            };
        }

        void reading_from_stream()
        {
            context["reading from file"] = () =>
            {
                it["should read first character"] = () =>
                {
                    inputStream.open_input("testfile.txt");

                    (inputStream.current_position.FileName as string).should_be("testfile.txt");
                    (inputStream.current_stream as StreamReader).should_not_be_null();

                    char c = inputStream.read_current_char();

                    c.should_be('s');
                };
            };
        }
    }
}