# PaintTogether

### Please note, this project is not intended to be used by anyone and is *very* messy!
(If you insist on attempting to build the project yourself, on linux atleast, you will need to install WINE and Winetricks, set a specific enviroment variable in your .bashrc file, and pray to a deity of your choice)

This project was a heavily bodged and messy multiplayer painting program i built with the goal of seeing just how much i could make with essentially zero external tutorials or researching. Just going off what i knew about painting programs and server designs.

The client uses a small game engine, monogame (Essentially just XNA framework), to handle the display and shader loading.
The server is made entirely from scratch using only builtin system libraries.

With the hindsight of working on it for just under a year, It's quite easy to see all the mistakes i made while trying to get everything to work.
In the client, organizing files like it's a terraria mod, creating the whole program and while forgetting undo/redo needs to be designed around, and similar mistakes are all obvious and i had to bodge my way around into a solution for them.
In the server, assigning each user a separate task handler but then using an entirely different collection of threads to actually process the packets, makes it fairly obvious this is my first ever attempt at any kind of multithreaded relay server.

For a first and completely uneducated attempt at both of these, i'm still quite proud i managed to make it mostly work.

## Folder info

PaintTogether -> Code and assets for the client program

PaintTogetherServer -> Code for the server program

Concepts -> My thought processes and (naive) design ideas i had while making this
