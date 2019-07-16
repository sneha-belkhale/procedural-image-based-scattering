# Procedural Image Based Scattering

Exploration into procedurally scattering cubes based on a reference image, and scaling them to compositionally balance each other. This project was made with Unity. 

<img src ="https://user-images.githubusercontent.com/17795014/61326476-b3e77180-a7cb-11e9-982f-7a00aae2a729.gif" width="75%">

## Description
Given a reference image and an arbitrary starting point, this algorithm aims to scatter points that flow along related colors of the image. The next cube’s position is chosen to be one of 8 neighboring points ( at a given step size ) that minimizes the following two values:

  * Difference in color value.
  
  * Dot product of the previous and current direction of motion. This is so that the cubes are inclined to move in a flowing pattern.

Then the cubes size is calculated by maximizing the equilibrium of the position and sizes of the previous N cubes, following the logic of the talk <a href ="https://vimeo.com/261901560">Creativity of Rules and Patterns: Designing Procedural Systems</a>.

The results are interesting, but in the future I would like to take this exploration and tune it for a specific goal, such as alien cities, forests, or caves. It would also be cool to optimize the problem for a first person view, instead of only thinking of the 2D top down scattering problem.

## Usage

Open the Demo scene in the Unity project, and press play. You can move around the sphere and press G to generate a new scattering starting at the cubes position. 

### Parameter Overview

**Basic**

`Reference Tex` - replace this image with the desired reference image. You should also replace the image in the `Base Plane` Material if you want to visualize the texture behind the generation.

`Cube Mat` - the material for the generated cubes. 

`Starting Pos Helper` - the game object who's position determines the starting point of the algorithm. 

`Generated Cube Count`- how many cubes to generate. 


**Scattering** 

`Flow Amount` - how much weight should be on the angular difference during minimization. 

`Step Size` - how far away the neighboring points should be sampled. 
Randomness - how much randomness to apply to the neighboring point sampling. 


**Scaling**

`Cubes To Balance` - the number of previous cubes to bring to equillibrium when calculating the current cube scale.

`Cycle Scale` - check this if you want the scales to be randomly cycled. 

## Results
<img src ="https://user-images.githubusercontent.com/17795014/61326558-e42f1000-a7cb-11e9-911e-beb604c174d7.png" width="75%">
<img src ="https://user-images.githubusercontent.com/17795014/61326497-c5307e00-a7cb-11e9-95be-efe7e46767a4.png" width="75%">
<img src ="https://user-images.githubusercontent.com/17795014/61326492-c2358d80-a7cb-11e9-8dbb-3668299e1a9c.png" width="75%">

## References 
<a href ="https://medium.com/@snayss/exploration-into-image-based-procedural-generation-unity-8f9fa7de10c1">Medium article about the exploration</a>.

<a href ="https://vimeo.com/261901560">Creativity of Rules and Patterns: Designing Procedural Systems</a>.

<a href ="https://www.jasondavies.com/plasma/">Infinite plasma fractal reference image</a>.




Check out https://codercat.tk for other projects :) 


