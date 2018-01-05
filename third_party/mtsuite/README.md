#mtsuite

**mtsuite** is a collection of disk utilities for Windows, optimized for
high performance on Solid State Drives (SSD).

Each program in the collection

* Uses all the available CPU cores to achieve maximum disk throughput, making
  it typically much faster than other disk utilities, espcecially when run
  on Solid State Drives (SSD).

* Supports long paths, i.e. paths greater than 260 characters.

* Supports Symbolic Links.

## Included utilities

The 4 programs included are

* **mtdel**: deletes a directory recursively. This is much faster than
               using *Windows Explorer* or even `rmdir /s /q`.

* **mtcopy**: copies a source directory recursively to a destination
                directory. This is similar to using `XCOPY /S` or
                `ROBOCOPY /S`.

* **mtmir**: same as mtcopy, except extra files not present in the
               source are deleted from the destination. This is
               similar to `ROBOCOPY /MIR`.

* **mtinfo**: displays file system statistics of a directory: number
                of files, number of subdirectories, size, etc. This can
                be 20x faster than using the *Properties* menu 
                in *Windows Explorer* to find the size of a folder.

## Symbolic Links support

A File Symbolic Link is a special file on disk that points to a another
file in a different location on disk (the "target"). When reading the
"contents" of a symbolic link, Windows actually reads the contents on
the target file.

Similarly, a Directory Symbolic Link is a special file on disk that points
to another directory on disk (the "target"). When enumerating files
in a directory symbolic link, Windows actually enumerates files from
the target directory.

In both cases, what is stored on disk by Windows for Symbolic Links is

* The type of link (file/directory)
* The path to the target file/directory, either as an absolute or relative
  path. A relative path is relative to the location of the link itself.

File and Directory Symbolic Links have been available since *Windows Vista*.

### Example

Suppose we have the directory structure below and we want to copy it
into another directory, let's say `c:\test-copy`.

```
c:\test (directory)
  foo (directory)
    bar.rlink.txt (symbolic link to "..\bar.txt")
  foo2 (directory)
      foo.link (symbolic link to "..\foo")
  bar.txt (file)
```

Both `XCOPY /S` and `ROBOCOPY /MIR` will copy the target of the symbolic
links, so the result of a copy would be:

```
c:\test-copy (directory)
  foo (directory)
    bar.rlink.txt (file, same content as "c:\test\bar.txt")
  foo2 (directory)
      foo.link (directory, same content as c:\foo)
        bar.rlink.txt (file, same content as "c:\test\bar.txt")
  bar.txt (file)
```

So, in essence, Symblic Links have been "expanded" to their target
content, and are not Symbolic Links anymore in the new copy.

With mtcopy and mtmir, Symbolic Links are copied as links, not as
their target contents, so the end result of running the command

```
mtcopy c:\test c:\test-copy
```

will be:

```
c:\test-copy (directory)
  foo (directory)
    bar.rlink.txt (symbolic link to "..\bar.txt")
  foo2 (directory)
      foo.link (symbolic link to "..\foo")
  bar.txt (file)
```


## Coming soon:

* Benchmarks
