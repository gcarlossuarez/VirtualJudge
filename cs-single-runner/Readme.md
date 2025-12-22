You can see the names of all your Docker images with the following command:
```bash
docker images
```

The "REPOSITORY" column displays the name of each image, and "TAG" displays the
tag (for example, latest). Use that name and tag to run or inspect the image.


For example, if you want to see the created Docker containers:
```bash
docker images
```

```text
virtualbox@virtualbox-VirtualBox:~/VirtualJudge/cs-single-runner$ docker images
REPOSITORY         TAG       IMAGE ID       CREATED          SIZE
cs-single-runner   1         2fad4c9d1690   11 minutes ago   1.78GB
```


Now, to inspect the directory created for this virtual container, without running
run_single.sh

```bash
docker run --rm -it --entrypoint bash 2fad4c9d1690
```


This will be the result:

sandbox@37f7f39a1c8b:~$


The contents of the root are inspected
```bash
ls
```

```text
sandbox@37f7f39a1c8b:~$ ls
in  run_single.sh  template  tmp
```


This is the content; you can navigate to these directories and view the files.

To exit the Docker image, type the `exit` command in the bash shell.
```bash
exit
```
