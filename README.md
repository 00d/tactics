WIP tactics game (in the spirit of Fire Emblem), using XNA/Monogame. Not yet playable, but there is some code to speak of! Currently implemented features:

- Multi-layer tile rendering engine which accepts output from the [Tiled Map Editor](http://www.mapeditor.org)
- Unit pathfinding based on tile passability flags set by map editor
- Unit selection and movement UI (with the little range flower overlay)
- Turn ending mechanics

Some speculative design notes can be found [here](https://docs.google.com/document/d/183HRH6jlWcrtIyTa1PrOgKRLWjQlycGBQ2KzZed7vFQ/edit#)

Included assets are for testing purposes only; all credit goes to their original creators.

### Building

While this should run perfectly well under [Monogame](http://www.monogame.net), until they finish their content pipeline you'll still need Windows and Visual Studio for development purposes, as detailed [here](http://blogs.msdn.com/b/tarawalker/archive/2012/12/04/windows-8-game-development-using-c-xna-and-monogame-3-0-building-a-shooter-game-walkthrough-part-1-overview-installation-monogame-3-0-project-creation.aspx). The free Express version of VS works fine.
