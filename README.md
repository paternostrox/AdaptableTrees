# AdaptableTrees
Repository for AdaptableTrees project, a product of my graduation thesis.
 
Adaptable Trees is a mesh synthesis tool for Unity. It is useful for creating beautiful customizable trees and bushes (or some other branching structure) considering obstacles in any preexisting 3D model. Until now, the tool does not support the creation of foliage.

Full description:

Based on the Space Colonization algorithm, the method uses multiple voxelizations to identify obstacles in a 3D environment and comprehend different levels of detail. The method uses a flood fill algorithm to clusterize free voxels and reliably identify obstacles. The purpose of this work is to propose a good alternative to tree synthesis in large amounts and low cost, automating tree adaptability and reducing the need for meticulous artistic controls, without demanding from the user any artistic or coding background.

Basic setup:

1) The user imports a 3D model and sets one or more voxelizations for the model. It is possible to set different voxelizations for different areas.
![vox-voxset](https://user-images.githubusercontent.com/19597048/232611805-1732fb63-a443-4f49-b60e-4edd9440738c.png)

2) The voxelizations are generated by flood fill, providing a discrete version of the model for the tree generation algorithm.
![show-occupied3](https://user-images.githubusercontent.com/19597048/232615063-c8b71861-30de-40f0-9acf-06a0d023bbd8.png)

3) The user picks the starting parameters for trees and clicks a location for one to be built on.
![tree-over-obstacle1](https://user-images.githubusercontent.com/19597048/232607713-77410db1-45fc-438e-adba-22af045718f7.png)

4) The volume of the future tree crown gets checked for free voxels by flood fill in the voxelization containing it. The resulting crown "cloud" then gets populated with points which will serve as attractors for the mesh generation.
![tree-over-obstacle2](https://user-images.githubusercontent.com/19597048/232607751-3fa75b13-3435-49da-ac9c-e14209e2896f.png)

5) The tree mesh is procedurally generated (and animated) based on the space colonization algorithm and a few extra constraints. If the user is not satisfied by the result, parameters can be altered and a new tree can be generated, also respecting the obstacles.
![tree-over-obstacle3](https://user-images.githubusercontent.com/19597048/232607769-d675287e-aff9-4e5f-b8b4-33eaba458e18.png)

   Make sure to play with the parameters, tiny ajustments can generate very different structures!
![tree-adjust-before](https://user-images.githubusercontent.com/19597048/232617707-493d08fd-c54d-4db1-9881-fb7412735b7e.png)
![tree-adjust-after](https://user-images.githubusercontent.com/19597048/232617717-54588cf0-70a9-41f6-b23f-902a6df942f2.png)

6) When you are done, you can also export the new forested environment to any other engine or modelling tool using Unity's FBX Exporter.

