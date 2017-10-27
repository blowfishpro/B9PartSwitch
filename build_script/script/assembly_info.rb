#!/usr/bin/env ruby

require_relative '../version/tag_version'

project_name = ARGV[0]

raise 'No project name specified!' if project_name.nil? || project_name.empty?

project_name = project_name.strip

version = tag_version

puts "Generating AssemblyInfo for project '#{project_name}' with version #{version.full_version}"

attributes_data = {
  'AssemblyVersion' 	  => ["\"#{version.full_version}\""],
  'AssemblyFileVersion' => ["\"#{version.full_version}\""],
  'KSPAssembly'		   	  => ["\"#{project_name}\"", version.major, version.minor],
}

attributes = attributes_data.map { |key, val| "[assembly: #{key}(#{val.join(', ')})]" }

assembly_info_in = File.read('files/AssemblyInfo.cs.erb')

assembly_info_elements = [assembly_info_in] + attributes + [nil]

assembly_info_out = assembly_info_elements.join("\n")

assembly_info_dir = File.join(project_name, 'Properties')
assembly_info_file = File.join(assembly_info_dir, 'AssemblyInfo.cs')

Dir.mkdir(assembly_info_dir) unless Dir.exist? assembly_info_dir

puts "Writing AssemblyInfo to '#{assembly_info_file}'"
File.open(assembly_info_file, 'w+') { |f| f.write(assembly_info_out) }

puts 'Done generating AssemblyInfo'
