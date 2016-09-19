#!/usr/bin/env ruby

require_relative '../version/tag_version'

version = get_tag_version
project_name = ENV['PROJECT_NAME']

attributes_data = {
  'AssemblyVersion' 	=> ["\"#{version.full_version}\""],
  'AssemblyFileVersion' => ["\"#{version.full_version}\""],
  'KSPAssembly'		 	=> ["\"#{project_name}\"", version.major, version.minor],
}

attributes = attributes_data.map { |key, val| "[assembly: #{key}(#{val.join(', ')})]" }

assembly_info_in = File.read('files/AssemblyInfo.cs.in')

assembly_info_elements = [assembly_info_in] + attributes + [nil]

assembly_info_out = assembly_info_elements.join("\n")

assembly_info_dir = File.join(project_name, 'Properties')

Dir.mkdir(assembly_info_dir) unless Dir.exists? assembly_info_dir
File.open(File.join(assembly_info_dir, 'AssemblyInfo.cs'), 'w+') { |f| f.write(assembly_info_out) }
