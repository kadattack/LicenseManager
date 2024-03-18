#!/bin/bash

# Define the directory to start the recursive search
directory="./"

# Define the string to be replaced and its replacement
search_string="Client"
replace_string="Client"

# Use find to locate all files and then use sed to replace the string
find "$directory" -type f -exec sed -i "s/$search_string/$replace_string/g" {} +

