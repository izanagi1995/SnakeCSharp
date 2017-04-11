using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Snake
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.CursorVisible = false;
            Board b = new Board(20, 20);
            Snake s = new Snake();
            b.setSnake(s);

            Selector selSnake = new Selector("Type de serpent?");
            selSnake.question.addAnswer("====", "Green", ConsoleColor.Green);
            selSnake.question.addAnswer("====", "Blue", ConsoleColor.Blue);
            selSnake.question.addAnswer("====", "Yellow", ConsoleColor.Yellow);

            selSnake.start().Join();
            Console.BackgroundColor = ConsoleColor.Black;
            Console.Clear();

            Selector selDiff = new Selector("Difficulté?");
            selDiff.question.addAnswer("Facile", "2", ConsoleColor.Green);
            selDiff.question.addAnswer("Moyen", "10", ConsoleColor.Yellow);
            selDiff.question.addAnswer("Difficile", "20", ConsoleColor.Red);
            selDiff.question.addAnswer("HARDCORE!!", "80", ConsoleColor.DarkRed);

            selDiff.start().Join();

            Console.BackgroundColor = ConsoleColor.Black;

            string color = selSnake.question.getAnswer();
            s.color = (ConsoleColor)(Enum.Parse(typeof(ConsoleColor), color));

            int diff = int.Parse(selDiff.question.getAnswer());
            b.difficulty = diff;

            Thread updateThread = new Thread(b.loop);
            updateThread.Start();

            Board.InputHandler inputHandler = new Board.InputHandler(s);
            Thread inputThread = new Thread(inputHandler.handleInput);
            inputThread.Start();


            updateThread.Join();
        }
    }

    class Selector
    {
        public Question question { get; set; }

        public class Question {

            public class Answer {
                public Answer(string label, string value, ConsoleColor color)
                {
                    this.label = label;
                    this.value = value;
                    this.color = color;
                }

                public string label { get; set; }
                public string value { get; set; }
                public ConsoleColor color { get; set; }



            }

            public string question { get; set; }
            public List<Answer> answers { get; set; }
            public int selection { get; set; }

            public Question(string question)
            {
                this.question = question;
                this.answers = new List<Answer>();
            }

            public void addAnswer(string label, string value, ConsoleColor color) {
                this.answers.Add(new Answer(label, value, color));
            }

            public string getAnswer() {
                return this.answers.ElementAt(selection).value;
            }

        }

        internal class InputHandler
        {
            private Selector sel;
            private bool done = false;

            public InputHandler(Selector s)
            {
                this.sel = s;
            }

            public void handleInput()
            {
                while (!done)
                {
                    switch (Console.ReadKey(true).Key)
                    {
                        case ConsoleKey.UpArrow:
                            sel.question.selection -= 1;
                            sel.redraw();
                            break;
                        case ConsoleKey.DownArrow:
                            sel.question.selection += 1;
                            sel.redraw();
                            break;
                        case ConsoleKey.Enter:
                            done = true;
                            break;
                    }
                }

            }
        }

        private void redraw()
        {
            Console.SetCursorPosition(0, 0);
            Console.ForegroundColor = ConsoleColor.White;
            Console.BackgroundColor = ConsoleColor.Black;
            Console.WriteLine(this.question.question);
            for (int i = 0; i < this.question.answers.Count; i++) {
                if (this.question.selection == i)
                {
                    Console.BackgroundColor = ConsoleColor.White;
                }
                else {
                    Console.BackgroundColor = ConsoleColor.Black;
                }
                Console.ForegroundColor = (this.question.answers[i].color);
                Console.WriteLine(this.question.answers[i].label);
            }
        }

        public Selector(string question) {
            this.question = new Question(question);
        }

        public Thread start()
        {
            this.redraw();
            InputHandler ih = new InputHandler(this);
            Thread t = new Thread(ih.handleInput);
            t.Start();
            return t;
        }

        
    }

    

    class Board {
        private int heigth;
        private int width;
        private Feed food;
        private Snake snake;
        public int difficulty;

        private int score = 0;

        public Board(int w, int h) {
            this.width = w;
            this.heigth = h;
            this.doFood();
        }

        internal class InputHandler
        {
            private Snake snake;

            public InputHandler(Snake s)
            {
                this.snake = s;
            }

            public void handleInput()
            {
                while (true)
                {
                    switch (Console.ReadKey(true).Key)
                    {
                        case ConsoleKey.LeftArrow:
                            snake.setDir(Direction.GAUCHE);
                            break;
                        case ConsoleKey.RightArrow:
                            snake.setDir(Direction.DROITE);
                            break;
                        case ConsoleKey.UpArrow:
                            snake.setDir(Direction.HAUT);
                            break;
                        case ConsoleKey.DownArrow:
                            snake.setDir(Direction.BAS);
                            break;
                    }
                }

            }
        }

        public void setSnake(Snake s) {
            this.snake = s;
        }

        public void loop() {
            while (!this.snake.dead) {
                this.updateSnake();
                this.print();
                Thread.Sleep(500/difficulty);
            }
        }

        public void doFood() {
            this.food = Feed.newFeed(this.width, this.heigth);
        }

        public void updateSnake() {
            if (snake.update(this.food)) {
                this.score += 1;
                this.doFood();
            }
            
        }

        public void print() {
            if (!this.snake.dead) {
                Console.SetCursorPosition(0, 0);
                for (int y = 0; y < this.heigth; y++)
                {
                    for (int x = 0; x < this.width; x++)
                    {
                        Console.SetCursorPosition(x * 2, y);
                        if (this.food.printableAt(x, y))
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.Write(" " + this.food.print());
                        }
                        else if (this.snake.printableAt(x, y))
                        {
                            Console.ForegroundColor = this.snake.color;
                            Console.Write(" " + this.snake.print());
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.White;
                            Console.Write(" .");
                        }
                    }
                    Console.WriteLine();
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.SetCursorPosition((this.width + 1) * 2, this.heigth / 2);
                    Console.WriteLine("Score : {0}", this.score);

                    Console.SetCursorPosition((this.width + 1) * 2, (this.heigth / 2) + 1);
                    Console.WriteLine("Longueur : {0}", this.snake.getSize());
                }
            }
            
        }

        
    }

    enum Direction
    {
        HAUT, BAS, GAUCHE, DROITE
    }

    class Snake {
        int xHead;
        int yHead;
        Direction dir;
        List<BodyPart> body = new List<BodyPart>();

        public ConsoleColor color { get; set; }
        public bool dead { get; internal set; }

        public Snake()
        {
            this.dead = false;
            this.xHead = 10;
            this.yHead = 10;
            this.body = new List<BodyPart>();
            this.dir = Direction.DROITE;
            this.body.Add(new BodyPart(9, 10));
        }

        public int getSize() {
            return this.body.Count + 1;
        }

        public char print() {
            return '=';
        }

        public bool printableAt(int x, int y)
        {
            bool h =  this.xHead == x && this.yHead == y;
            if (!h)
            {
                var p = this.body.Select(b => b.printableAt(x, y));
                p = p.Where(b => b == true);
                return p.Count() > 0;
            }
            else {
                return h;
            }
        }

        public bool update(Feed f)
        {
            bool fed = true;
            if (this.body.FindAll(b => b.x == this.xHead && b.y == this.yHead).Count > 0)
            {
                this.dead = true;
            }
            this.body.Insert(0, new BodyPart(this.xHead, this.yHead));
            if (!(f.getX() == this.xHead && f.getY() == this.yHead)) {
                this.body.RemoveAt(this.body.Count - 1);
                fed = false;
            }
            
            switch(this.dir){
                case Direction.BAS:
                    this.yHead += 1;
                    break;
                case Direction.HAUT:
                    this.yHead -= 1;
                    break;
                case Direction.GAUCHE:
                    this.xHead -= 1;
                    break;
                case Direction.DROITE:
                    this.xHead += 1;
                    break;
            }
            return fed;
        }

        internal void setDir(Direction d)
        {
            this.dir = d;
        }

        internal void setColor(object v)
        {
            throw new NotImplementedException();
        }
    }

    class BodyPart : Printable {
        public int x;
        public int y;

        public BodyPart(int xMax, int yMax)
        {
            this.x = xMax;
            this.y = yMax;
        }

        public char print()
        {
            return '=';
        }

        public bool printableAt(int x, int y)
        {
            return this.x == x && this.y == y;
        }

    }

    class Feed : Printable{
        int x;
        int y;

        public Feed(int xMax, int yMax) {
            Random r = new Random();
            this.x = r.Next(xMax);
            this.y = r.Next(yMax);
        }

        public char print() {
            return 'X';
        }

        public static Feed newFeed(int xMax, int yMax) {
            Feed f = new Feed(xMax, yMax);
            return f;
        }

        public bool printableAt(int x, int y) {
            return this.x == x && this.y == y;
        }

        internal int getX()
        {
            return this.x;
        }

        internal int getY()
        {
            return this.y;
        }
    }

    interface Printable {
        char print();
        bool printableAt(int x, int y);
    }

}
