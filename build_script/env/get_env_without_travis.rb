#!/usr/bin/env ruby

require 'yaml'

vars = YAML.safe_load(File.read('.travis.yml'))['env']['global']

puts "export TRAVIS_BUILD_DIR='#{Dir.pwd}'"

vars.each do |var|
  puts "export #{var}"
end
