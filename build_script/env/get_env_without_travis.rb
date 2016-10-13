#!/usr/bin/env ruby

require 'yaml'

vars = YAML.load(File.read('.travis.yml'))['env']['global']

vars.each do |var|
  puts "export #{var.gsub('"', '')}"
end
