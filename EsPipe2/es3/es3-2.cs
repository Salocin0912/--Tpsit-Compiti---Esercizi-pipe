#include <stdio.h>
#include <stdlib.h>
#include <fcntl.h>
#include <sys/stat.h>
#include <unistd.h>
#include <string.h>

#define FIFO_12 "/tmp/fifo_1_2" // Per ricevere
#define FIFO_21 "/tmp/fifo_21" // Per inviare

int main() {
    // Assicura l'esistenza delle FIFO
    mkfifo(FIFO_12, 0666);
    mkfifo(FIFO_21, 0666);

    printf("[Chat 2] In attesa della connessione (ordine speculare: prima lettura, poi scrittura)...\n");

    // ORDINE SPECULARE ANTI-DEADLOCK: Prima apre in lettura, poi in scrittura
    int fd_read = open(FIFO_12, O_RDONLY);
    int fd_write = open(FIFO_21, O_WRONLY);

    char msg_out[256];
    char msg_in[256];

    while (1) {
        // Prima riceve il messaggio dall'altra chat
        memset(msg_in, 0, sizeof(msg_in));
        read(fd_read, msg_in, sizeof(msg_in));

        if (strncmp(msg_in, "EXIT", 4) == 0) {
            printf("[Chat 2] L'altro utente e' uscito.\n");
            break;
        }

        printf("Chat 1 scrive: %s", msg_in);

        // Legge da tastiera la propria risposta
        printf("Tu (Chat 2): ");
        if (fgets(msg_out, sizeof(msg_out), stdin) == NULL) break;

        write(fd_write, msg_out, strlen(msg_out) + 1);

        if (strncmp(msg_out, "EXIT", 4) == 0) {
            printf("[Chat 2] Uscita dalla chat.\n");
            break;
        }
    }

    close(fd_read);
    close(fd_write);

    return 0;
}