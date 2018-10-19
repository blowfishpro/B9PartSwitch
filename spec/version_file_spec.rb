require 'spec_helper'
require 'json'
require 'rest-client'

RSpec.describe 'B9PartSwitch.version' do
  it 'exists, is valid json, and is formatted correctly' do
    expect(File.file?('release/GameData/B9PartSwitch/B9PartSwitch.version')).to be(true)

    raw_contents = File.read('release/GameData/B9PartSwitch/B9PartSwitch.version')

    parsed_json = JSON.parse(raw_contents)

    name = parsed_json['NAME']
    download = parsed_json['DOWNLOAD']
    url = parsed_json['URL']
    github = parsed_json['GITHUB']
    version = parsed_json['VERSION']
    ksp_version = parsed_json['KSP_VERSION']

    expect(name).not_to be_nil
    expect(download).not_to be_nil
    expect(url).not_to be_nil

    expect(RestClient.head(download).code).to be_between(200, 299)
    # expect(RestClient.head(url).code).to be_between(200, 299)

    expect(github).not_to be_nil

    github_username = github['USERNAME']
    github_repository = github['REPOSITORY']

    expect(github_username).not_to be_nil
    expect(github_repository).not_to be_nil

    expect(RestClient.head("https://github.com/#{github_username}/#{github_repository}").code).to be_between(200, 299)

    expect(version).to match(/\d+(?:\.\d+)*/)
    expect(ksp_version).to match(/\d+(?:\.\d+)*/)
  end
end
