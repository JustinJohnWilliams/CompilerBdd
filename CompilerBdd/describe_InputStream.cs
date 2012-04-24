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

        private List<string> lines { get; set; }

        public void open_input(string fileName)
        {
            var path = GetFullFilePath(fileName);

            if (File.Exists(path))
            {
                current_position.FileName = fileName;
                current_stream = new StreamReader(path);
                
                lines = current_stream.ReadToEnd().Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries).ToList();
            }
            else throw new FileNotFoundException("File {0} does not exist.".With(path));
        }

        public char read_next_char()
        {
            if (end_of_input())
            {
                throw new EndOfStreamException();
            }

            char result;

            if (EndOfLine())
            {
                result = '\n';
                current_position.move_row_by(1);
            }
            else
            {
                result = lines[current_position.Line - 1]
                                       .ElementAt(current_position.Column);

                current_position.move_column_by(1);
            }            

            return result;
        }

        public bool end_of_input()
        {
            return (current_position.Line - 1 == lines.Count) ? true : false;
        }

        public void close()
        {
            if (this.current_stream != null) current_stream.Close();
        }

        private string GetFullFilePath(string fileName)
        {
            return "{0}/Data/{1}".With(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), fileName);
        }

        private bool EndOfLine()
        {
            return current_position.Column == lines[current_position.Line - 1].Length;
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
            before = () => inputStream.open_input("testfile.txt");

            context["reading from file"] = () =>
            {
                it["should read characters on first line"] = () =>
                {
                    ReadFirstLine();
                };

                it["should move to next line after reaching end of line"] = () =>
                {
                    ReadFirstLine();
                    ReadSecondLine();
                };
                
                it["should throw and end of file exceptiopn if no more file to read"] = expect<EndOfStreamException>(() =>
                {
                    ReadFile();
                    ReadNext();
                });

                it["should return true if current position exceeds input text"] = () =>
                {
                    ReadFile();
                    
                    ((bool)inputStream.end_of_input()).should_be_true();
                };

            };
        }

        void printing_out_file()
        {
            before = () => inputStream.open_input("testfile.txt");

            //remove the 'x' and it will print file
            xit["should print out file"] = () =>
            {
                while (!inputStream.end_of_input())
                {
                    Console.Write(ReadNext());
                }
            };
        }

        private char ReadNext()
        {
            return inputStream.read_next_char();
        }

        private void ReadFirstLine()
        {
            ReadNext().should_be('t');
            ReadNext().should_be('e');
            ReadNext().should_be('s');
            ReadNext().should_be('t');

            ReadNext().should_be(' ');
            ReadNext().should_be('t');
            ReadNext().should_be('e');
            ReadNext().should_be('x');
            ReadNext().should_be('t');

            ReadNext().should_be(NewLine());
        }

        private void ReadSecondLine()
        {
            ReadNext().should_be('l');
            ReadNext().should_be('i');
            ReadNext().should_be('n');
            ReadNext().should_be('e');
            ReadNext().should_be(' ');
            ReadNext().should_be('2');

            ReadNext().should_be(NewLine());
        }

        private void ReadThirdLine()
        {
            ReadNext().should_be('¿');
            ReadNext().should_be('`');
            ReadNext().should_be('~');
            ReadNext().should_be('!');
            ReadNext().should_be('@');
            ReadNext().should_be('#');
            ReadNext().should_be('$');
            ReadNext().should_be('%');
            ReadNext().should_be('^');
            ReadNext().should_be('&');
            ReadNext().should_be('*');
            ReadNext().should_be('(');
            ReadNext().should_be(')');
            ReadNext().should_be('_');
            ReadNext().should_be('+');

            ReadNext().should_be(NewLine());
        }

        private void ReadFile()
        {
            ReadFirstLine();
            ReadSecondLine();
            ReadThirdLine();
        }

        private char NewLine()
        {
            return '\n';
        }
    }
}