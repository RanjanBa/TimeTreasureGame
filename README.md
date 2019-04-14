## Time Treasure Game ##
* #### [Project overview](#project-overview) ####
* #### [Gameplay overview](#gameplay-overview) ####
* #### [Rules of the Cards](#rules-cards) ####
* #### [How to play the game](#how-to-play-in-platform) ####
* #### [Tools Used](#tools) ####


#### <a name="project-overview"/> 1. Project overview ####
>__Time Treasure__ is a card game designed to teach primary school students about the world time zone and how the time depends on the latitude and longitude. This game can be played among two to four players at a time. The aim of the project was to build a software game from already designed physical card game which can be played online and offline with friends on android mobile platform.

#### <a name="gameplay-overview"/> 2. Gameplay overview ####
>In this section, I will give you quick overview of gameplay of the __Time Treasure__ card game. The game can be played among 2 to 4 players. The main of objective of the game is collect as many treasures and coin as possible from the map. The board have the picture of world map where 24 longitude line and 11 latitude lines are present. Player can move from one cross section points of the lines. There are many coins and treasure boxes at the cross sections of the lines of longitude and latitude and player can collect that coins and treasure box by going to that cross-section point. When there is no coins and treasure boxes left on the map, the player who collects the maximum coins and treasures will win the game.

![Gameplay board](https://markdown-here.com/img/icon256.png)

#### <a name="rules-cards"/> 3. Rules of the cards ####
>There are four types of cards:
>* 24 Hour cards :
>   * ranges from 1AM to 12AM and 1PM to 12PM
>* 22 Power cards :
>   * 2 Longitude Master cards
>   * 4 GMT Master cards
>   * 2 Master cards
>   * 2 Three Point cards
>   * 6 +x hour Cards (where x ranges from 1 to 6)
>   * 6 -x hour cards (where x ranges from 1 to 6)
>* 24 Trap cards :
>   * ranges from -11 to 12
>* 70 Fuel cards

>* __Hour card :__ By this cards player can move from one longitude to another longitude along the same latitude. For example, player has _2AM_ card, then he can move his pawn from current longitude to that longitude where local time is _2AM_.

>* __Power Card :__
>   * _Longitude Master card :_ By this card player can collect all treasures and coins present on the longitude where player is currently present.
>   * _GMT Master card :_ By this card player can change the time of the GMT0 longitude to any time.
>   * _Master Card :_ By this card player can move to any longitude without changing the latitude.
>   * _There Point card :_ By this card player can get there point.
>   * _+x Hour Card :_ By this card player can jump x steps forward. For example, player has +3hr card, then he can jump 3 step forward along the same latitude.
>   * _-x Hour Card :_ By this card player can jump x steps backward. For example, player has -3hr card, then he can jump 3 step backward along the same latitude.

>* __Fuel Card :__ By this card player can move along the same longitude i.e. player can change the latitude by playing this card. For example, player can jump 3 step upward or downward by playing 3 fuel cards.

>* __Trap Card :__ By this card player can trap another player i.e. player can play this card to get fuel card from another player. For example, player has _GMT-2 trap card_ and other player is at -2 longitude, then player can play that card to get fuel from another player.


#### <a name="how-to-play-in-platform"/> 4. How to play the game ####
>To play the game in android platform, you have to install the \*.apk file  and in widnows pc you play the by running the TimeTreasure.exe executable file which are present at Build folder. Currently you can play online multiplayer only in android platform.

#### <a name="tools"/> 5. Tools used ####
>
>  * Unity Game Engine
>  * Visual Studio 2017
>  * Firebase (for online multiplayer)

>  * __Unity Game Engine :__ It is a software-development environment designed for game developers to build video games. By this engine we can build games for any platform like android, pc, console etc.

>  * __Visual Studio 2017 :__ It is an integrated development environment used to developed computer programs, mobile apps.

>  * __Firebase :__ It is a mobile and web application development platform. It is google product which offers so many features like realtime database, basic analytics, authentication, storage etc. I used it for realtime database and authentication purpose.
It is used because it has some functionality like listening to changes in the database and get the data changes in realtime i.e. it can sync the data across the client platform.

#### <a name="how-to-use"/> 5. How to use the project ####
>
