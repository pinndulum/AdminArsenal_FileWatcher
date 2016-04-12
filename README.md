# Admin Arsenal FileWatcher

This project is the result of an excercise used to indicate how requirements are interpreted by developers. It only took an hour or so to complete on July 20 2014.

In this exercise I created a command-line program using C# to watch for text files created or modified in a given directory, and then output information about the changes.

Here are the requirements as they were provided.

- The program takes 2 arguments, the directory to watch and a file pattern, example: program.exe "c:file folder" *.txt
- The path may be an absolute path, relative to the current directory, or UNC.
- Use the modified date of the file as a trigger that the file has changed.
- Check for changes every 10 seconds.
- When a file is created output a line to the console with its name and how many lines are in it.
- When a file is modified output a line with its name and the change in number of lines (use a + or - to indicate more or less).
- When a file is deleted output a line with its name.
- Files will be ASCII or UTF-8 and will use Windows line separators (CR LF).
- Multiple files may be changed at the same time, can be up to 2 GB in size, and may be locked for several seconds at a time.
- Use multiple threads so that the program doesn't block on a single large or locked file.
- Program will be run on Windows 7 
- File names are case insensitive.
- Lastly, please create a text file and answer the following questions (with one or two sentences each) :
  * What is your favorite type of development task and why?
  * What part of an application do you prefer working on and why? (such as user interface, database, server components, etc.)
  * If you could only eat one kind of food while programming, what would it be?

To answer the last requirement, locate the file name Q&A.txt in the src directory.

found another who completed the same excecise [here](https://github.com/mwingle/watcher)
