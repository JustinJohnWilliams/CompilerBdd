using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NSpec;

namespace CompilerBdd
{
    public class Position
    {
        public int Line { get; private set; }
        public int Column { get; private set; }
        public string FileName { get; set; }

        public Position()
        {
            Line = 1;
            Column = 0;
        }

        public Position(dynamic position)
        {
            Line = position.Line;
            Column = position.Column;
            FileName = position.FileName;
        }

        public void move_column_by(int x)
        {
            Column += x;
        }

        public void move_row_by(int y)
        {
            Line += y;
            Column = 0;
        }

        public string current_position()
        {
            return "{0}:{1}:{2}: ".With(FileName, Line, Column);
        }
    }

    public class describe_Position : nspec
    {
        dynamic position;

        void before_each()
        {
            position = new Position();
        }

        void input_file_actions()
        {
            context["when getting position of file"] = () =>
            {
                before = () =>
                {
                    //nothing needs to be ran before tests for this context
                };

                it["should return current position at 1, 0"] = () =>
                {
                    (position as Position).should_be(1, 0);
                };

                context["when incrementing the column should increase column and leave row the same"] = () =>
                {
                    it["should return 1, 1 when incrementing column by 1"] = () =>
                    {
                        position.move_column_by(1);

                        (position as Position).should_be(1, 1);
                    };

                    it["should return 1, 10 when incrementing column by 10"] = () =>
                    {
                        position.move_column_by(10);

                        (position as Position).should_be(1, 10);
                    };
                };

                context["when incrementing row should increase row and reset column to 0"] = () =>
                {
                    it["should return 2, 0 when incrementing row by 1"] = () =>
                    {
                        position.move_row_by(1);

                        (position as Position).should_be(2, 0);
                    };
                };
            };

            context["when copying position should return new instance of position with same values"] = () =>
            {
                before = () =>
                {
                    position.move_row_by(9);
                    position.move_column_by(10);
                    position.FileName = "testfile";
                };

                it["should return 10, 10, 'testfile' as a new instance"] = () =>
                {
                    dynamic position2 = new Position(position);

                    (position2 as Position).should_be(10, 10, "testfile");
                    (position2 as Position).should_not_be_same(position as Position);
                };
            };

            context["when printing out current position"] = () =>
            {
                before = () =>
                {
                    position.move_row_by(11);
                    position.move_column_by(4);
                    position.FileName = "testfile";
                };

                it["should print out 'testfile:12:4: '"] = () =>
                {
                    dynamic printout = position.current_position();

                    (printout as string).should_be("testfile:12:4: ");
                };
            };
        }
    }

    public static class position_extensions
    {
        public static void should_be(this Position position, int line, int column)
        {
            position.Column.should_be(column);
            position.Line.should_be(line);
        }

        public static void should_be(this Position position, int line, int column, string fileName)
        {
            position.Column.should_be(column);
            position.Line.should_be(line);
            position.FileName.should_be(fileName);
        }
    }
}