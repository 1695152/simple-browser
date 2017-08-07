# simple-browser
Mon objectif avec ce projet est d'offrir un navigateur internet facile à utiliser pour les "néophytes" ou simplement ceux qui veulent utiliser un navigateur internet simple sans les options standards qui peuvent être superflus. Mon public cible serait mes grands-parent, ma mère, etc.

Techniquement 
Un projet WPF qui utilise webBrowser pour afficher une page web

Fonctionalités
* Détecte si préfixe http est présent, si non ajoute https;
* Teste la validité d'un url avant de naviguer, si non valide redirige la requête en recherche
* Historique de session: les différents site visités sont stocké en mémoire et on peut naviguer dans les pages visités (bouton précédent & suivant)
* Maximisation de l'espace: les dimensions de la fenêtre sont égales à celle de la résolution de l'écran
