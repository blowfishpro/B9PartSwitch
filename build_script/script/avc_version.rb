#!/usr/bin/env ruby

require 'erubis'

require_relative '../version/tag_version'
require_relative '../version/ksp_version'

if ARGV.size != 2
  puts "usage: #{__FILE__} in_erb_file out_version_file"
  exit 1
end

version = tag_version

puts "#{__FILE__}: filling version info into ERB template"
puts "#{__FILE__}: ksp version: #{ksp_version}"
puts "#{__FILE__}: version: #{version}"

in_erb_file = ARGV[0]
out_version_file = ARGV[1]

erb = Erubis::Eruby.new(File.read(in_erb_file))

File.open(out_version_file, 'w+') do |f|
  f.write erb.result(version: version, ksp_version: ksp_version)
end
