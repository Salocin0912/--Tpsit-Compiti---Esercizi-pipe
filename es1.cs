#include <stdio.h>
#include <stdlib.h>
#include <unistd.h>

int main(void) {
    /*
     * Creiamo file descriptor fd[2]
     * fd[0] -> read
     * fd[1] -> write
     */
    int fd[2];

    //Apriamo la pipe, se ritorna -1 è avvenuto un errore
    if (pipe(fd) == -1) {
        perror("pipe");
        exit(-1);
    }

    // Duplichiamo il processo e quindi anche i file descriptor
    pid_t ritorno_fork = fork();
    if (ritorno_fork == -1) {
        perror("fork");
        exit(-2);
    }

    // Processo Figlio
    if (!ritorno_fork) {
        //Chiudiamo il file descriptor che non usiamo
        close(fd[0]);
        //Prepariamo il messaggio
        char msg[] = "Ciao padre";
        //Scriviamo nella pipe
        write(fd[1], msg, sizeof(msg));
        
        close(fd[1]);
        exit(0);
    }
    
    // Processo Padre
    close(fd[1]);
    
    //Prepariamo il buffer d'appoggio dove scriveremo i dati
    char buffer[100];
    //leggiamo dalla pipe
    read(fd[0], buffer, sizeof(buffer));
    
    close(fd[0]);

    printf("Messaggio dal figlio: %s\n", buffer);
    return 0;
}