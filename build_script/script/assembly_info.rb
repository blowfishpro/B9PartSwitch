#!/usr/bin/env ruby

require 'erubis'

require_relative '../version/tag_version'
require_relative '../version/ksp_version'

project_name = ARGV[0]

raise 'No project name specified!' if project_name.nil? || project_name.empty?

project_name = project_name.strip

version = tag_version

puts "Generating AssemblyInfo for project '#{project_name}' with version #{version.full_version}"

assembly_info_dir = File.join(project_name, 'Properties')
assembly_info_file = File.join(assembly_info_dir, 'AssemblyInfo.cs')

Dir.mkdir(assembly_info_dir) unless Dir.exist? assembly_info_dir

erb = Erubis::Eruby.new(File.read('files/AssemblyInfo.cs.erb'))

puts "Writing AssemblyInfo to '#{assembly_info_file}'"

File.open(assembly_info_file, 'w+') do |f|
  f.write erb.result(project_name: project_name, version: version, ksp_version: ksp_version)
end

puts 'Done generating AssemblyInfo'
