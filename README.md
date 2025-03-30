# 3C type mario kart

Projet d'études conception et réalisation de jeux-vidéos 2ème année

## Le projet

Le but de ce protoype est de construire une base 3C de véhicule en 3D, qui pourrait servir à commencer des jeux type Mario Kart.
Les points clés développés sont :
- Le contrôle du véhicule
- La gestion de la caméra
- Un système d'objets
- Le multijoueur local
- La gestion des courses
- Quelques mécaniques typiques (drift, patch turbo, terrain ralentissant)

Tous ces éléments sont paramétrables et / ou peuvent être étendus pour coller aux besoin d'un nouveau projet.

## Les contrôles
Le véhicule du joueur peut avancer, reculer, tourner, ainsi que drifter. Le déplacement s'effectue concrètement en recalculant la velocity du rigidbody à chaque FixedUpdate, en se basant sur les inputs du joueurs, l'état du véhicule, et des raycasts.
Cette approche permet au moteur de jeu de gérer une partie de la physique (par exemple les collisions) tout en gardant une maîtrise quasi-totale des déplacements. Ce n'est pas aussi élaboré que de coder soi-même une physique de bout-en-bout, mais cela permet de customiser certains aspects (par exemple le drift ou la gravité).

Touches pour deux joueurs (clavier uniquement) :

| Action          | Joueur 1 | Joueur 2                |
|-----------------|----------|-------------------------|
| Avancer         | Z        | Flèche haut             |
| Freiner/Reculer | S        | Flèche bas              |
| Tourner         | Q/D      | Flèche gauche/droite    |
| Drifter         | Espace   | 0 (pavé numérique)      |
| Utiliser item   | E        | Entrée (pavé numérique) |
| Pause           | Échap    | Échap                   |

## La caméra
Le module cinemachine a été utilisé pour pouvoir suivre le joueur de manière fluide, et donner des petits effets de style comme voir le côté du véhicule quand le joueur tourne.

## Les compétences character
Un système d'objet a été mis en place pour permettre d'ajouter facilement n'importe quel type d'objet en créant de nouveaux ScriptableObject.

## Les courses
Un circuit de test peut être joué à deux de manière compétitive : il faut suivre le tracé de la route et faire des tours le plus vite possible. Le jeu calcule si un joueur a bien suivi le circuit grâce à des checkpoints disséminés, et attribue des points selon la position du joueur quand il termine le nombre de tours nécessaire. Ces règles peuvent être ajustées via le GameManager en changeant le nombre de tours requis, la proportion de checkpoints à passer, et le nombre de points attribués quand on termine la course.