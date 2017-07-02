#!/usr/bin/env ruby

require 'date'
require 'erubis'

if ARGV.size != 2
  puts "usage: #{__FILE__} in_erb_file out_file"
  exit 1
end

in_erb_file = ARGV[0]
out_file = ARGV[1]

version = `git describe --tags`.strip
tag = ENV['TRAVIS_TAG']

erb = Erubis::Eruby.new(File.read(in_erb_file))

File.open(out_file, 'w+') do |f|
  f.write erb.result(version: version, tag: tag)
end
