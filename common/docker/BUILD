for i in glob("*.dockerfile"):
    filegroup(
        name=i,
        srcs=[i, "{dockerfile}.dockerignore".format(dockerfile=i)],
        visibility=["PUBLIC"]
    )

