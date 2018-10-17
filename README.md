## Duck Hunter VR Overview

### Demo Video
[![Demo](https://github.com/jthom330/Duck-Hunter-VR/blob/master/Images/Start_Screen.png)](https://youtu.be/1ctn2TvFcPI)

### Description
For my capstone project, I am building a Google Daydream app inspired by the the original NES Duck Hunt.
I chose this game to express the emotion of joy on a very personal level.  My earliest memories of playing 
video games with my family are of playing Duck Hunt in my parent's bedroom as a child.  I wanted this project to 
be something that I could share with them in order to recreate those old times.

The interactions for this VR experience are intentionally very simple.  The only controls are to look around using the 
Daydream headset and to aim and fire a shotgun using the Daydream remote.  Users will start at a title screen where 
they can view high scores and interact with a button to start the game.  Once a user clicks the start button, they will 
be moved to the area where the main game will take place.  

The game play consists of rounds of increasing difficulty where the user must hit a given number of ducks.  This goes on 
until the user fails, at which point high scores are recorded and they can restart or quit the application.

### Features

#### Duck AI
In order to make the game challenging, I needed to make the ducks move in a chaotic pattern.
I achieved this through a simple strategy of having the ducks contain a destination point 
that would move to a random point, within a range, each time they collided with it.  While the AI
is very simple, it works perfectly for this game. 

![Duck](https://github.com/jthom330/Duck-Hunter-VR/blob/master/Images/Duck_Model.png "Duck Model")


#### Animations
This game has three areas that contain animation: the ducks, the gun, and the UI.
I created all the animations in the game within Unity.  The gun has a kickback 
and pump animation that was created simply by shifting positions.  The ducks have a flying and
death animation that was created by changing the rotation on their wings and body.  Finally, I animated 
some of the UI elements using scale and rotation.  

![Animations](https://github.com/jthom330/Duck-Hunter-VR/blob/master/Images/Animation_Gun.png "Gun Animation")

#### Particle Effects 
I used particle systems just to draw attention to actions or add some visual interest.
For the primary mechanics, I used them to show the muzzle flash on the user's gun as well as 
emphasizing when a duck was hit.  In the background I used a particle system to display a smoke 
stack.

![Animations](https://github.com/jthom330/Duck-Hunter-VR/blob/master/Images/Shot.png "Gun Animation")

#### Low Poly Environments 
I wanted to create a beautiful environment so that players would have a pleasant view as 
they played.  Due to the resource constraints of mobile VR, I had to use low poly assets 
in order to fit everything I wanted into the scene.  I was able to make up for the lack 
of detail using bright colors and baked lighting.

![Map](https://github.com/jthom330/Duck-Hunter-VR/blob/master/Images/Map.png "Low Poly Map")

#### Score System
Since this game does not have an 'end' I wanted to incentivize plays with a scoring system.
The game keeps track of the top 3 high scores in order to provide a goal to strive for.  1000 
points are awarded for each duck hit multiplied by the number of shots left.

#### Simplified UI
For menus/prompts I used basic UI windows in world space, as demonstrated multiple times within 
the VR nano-degree program.  However, the traditional HUD elements took some more creativity.
It took a few iterations and some user feedback before I landed on showing what would normally
be HUD elements on a wooden sign within easy view of the player.

![UI](https://github.com/jthom330/Duck-Hunter-VR/blob/master/Images/UI_Board.png "Simple UI")

#### Sound 
I used Google's resonance audio for spacial audio within the game environment.  I applied spacial 
audio to the player's gun sound effects as well as the duck sounds.  Music and ambiance do not
use spacial audio.  

#### Physics
I use physics for a few things in Duck Hunter VR.  The shot gun shells ejected from the gun have 
force applied to them and are affected by gravity.  Similarly, ducks have gravity applied to them 
after being hit.  Lastly, I use sphere casts to actually shoot at the ducks.  

#### Full Game Loop
##### Intro
* Title Screen
* Fade to Black
* Move player to position
#### Gameplay
* Show player required number of hits for current round 
* Player fires at ducks
* If they complete the round, repeat with increased difficulty.  Else, show game over menu.

### Development 

![Level](https://github.com/jthom330/Duck-Hunter-VR/blob/master/Images/Level_Sketch.png "Level Sketch")
![UI](https://github.com/jthom330/Duck-Hunter-VR/blob/master/Images/UI_Sketch.jpg "UI Sketch")


#### Idea and Strategy 
While I detailed all of the notable game elements in the above feature section, my initial strategy 
and primary focus throughout development were on three game elements: The feel of the gun, the duck
behavior, and the environment.

##### Gun
I spent most of the early development tweaking the the gun to make sure it felt satisfying to use.
I started with animations, making sure the pump action and the recoil felt right without being distracting.
I was sure to have the sound effects line up with the animations I created.  I created the muzzle flash effect
by modifying a land-mine explosion effect I found on the asset store.  Finally, I went through a few iterations
of the actual shooting.  I started by using ray casts, but I found that the ray required too much precision.  I 
ended up switching to a sphere cast and changing the size until it felt right.

##### Duck Behavior
As mentioned above the bulk of the duck AI is based on moving toward a random point (within a set range) in space until
it collides with it, at which point it will move to a new random position.  However, there are other aspects as well.  A
duck will exit the scene if it is not hit after a set amount of time or if the player runs out of bullets.  Finally, a death 
animation and gravity is applied to a duck upon being hit by the player.  

##### Environment
Building the level took a bit longer than I thought it would, given that I was using all purchased assets.  After creating the 
basic layout, I found the level was very boring.  At this point the challenge was adding details to the scene while keeping a 
fairly low tri count.  I added trees, an animated water shader, a more colorful skybox, and some border elements like mountains 
and hills.  I went through a lot of adding and deleting until I struck the right balance.   

#### User Testing 
I had a few rounds of user testing with friends through out development.  There were two big takeaways from these sessions.
1. Originally, I had the player in the center of the scene, but this required a lot of head movement which was disorienting to 
my testers.  I moved the player position so that there was a more focused area for playing the game.
2. I could not come up with a good solution for how to display what are traditionally HUD UI elements like score, bullets, etc.
One of my testers recommended putting the elements on a sign or billboard near the user.  This solution turn out to be one of 
my favorite aspects of the final game.  

#### Dropped Features/Issues
Unfortunately, I could not get everything I wanted into the final game. Here are the features I had to cut.

* Clear water (too GPU intensive)
* Fish (moot without clear water)
* Bullet effects/decals on most objects (requires colliders on most objects which is computational expensive and time consuming)
* Different types of ducks (could not find assets)
* More stylized look (not enough time to look into)

Lastly here are a few things that I was not able to polish as much as I would have liked due to time constraints. 

* High score (Would have liked to have kept more info than the top 3 raw scores)
* Menu UI (The menus were done last, so I had no time to polish them)
* Lighting (imported models had UV issues which causes seams to show)
 


