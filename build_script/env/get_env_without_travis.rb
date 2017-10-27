#!/usr/bin/env ruby

require 'yaml'

vars = YAML.safe_load(File.read('.travis.yml'))['env']['global']

vars.each do |var|
  puts "export #{var}"
end
